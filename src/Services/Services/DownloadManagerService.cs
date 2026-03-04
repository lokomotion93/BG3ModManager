using DynamicData;
using DynamicData.Binding;

using ModManager.Models.App;

using System.Collections.Concurrent;
using System.Net.Http;
using System.Reactive.Disposables;

using System.IO;
using System.Reactive.Concurrency;

namespace ModManager.Services;

public class DownloadManagerService : IDownloadManagerService
{
	private readonly SourceCache<DownloadTask, Guid> _tasks = new(x => x.Id);

	public int ActiveDownloads { get; private set; }
	public int MaxSimultaneous { get; set; } = 2;

	private readonly ReadOnlyObservableCollection<DownloadTask> _downloads;
	public ReadOnlyObservableCollection<DownloadTask> Downloads => _downloads;

	private static readonly SortExpressionComparer<DownloadTask> ByOldestStarted = SortExpressionComparer<DownloadTask>
		.Ascending(x => x.TimeStarted != DateTime.MinValue)
		.ThenByAscending(x => x.TimeStarted);

	private readonly ConcurrentQueue<DownloadTask> _downloadQueue = [];

	private static async Task ReadWriteContentStreamAsync(Stream contentStream, FileStream outputStream, CancellationToken token)
	{
		var totalRead = 0L;
		var totalReads = 0L;
		var buffer = new byte[8192];
		var isMoreToRead = true;

		while (isMoreToRead)
		{
			var read = await contentStream.ReadAsync(buffer, token);
			if (read == 0)
			{
				isMoreToRead = false;
			}
			else
			{
				await outputStream.WriteAsync(buffer.AsMemory(0, read), token);

				totalRead += read;
				totalReads += 1;
			}
		}
	}

	private static async Task<Stream> OpenUriAsStreamAsync(Uri downloadUrl, Dictionary<string,string> headers, CompositeDisposable disposables, CancellationToken token)
	{
		using var httpClient = new HttpClient();
		disposables.Add(httpClient);
#if DEBUG
		httpClient.Timeout = TimeSpan.FromSeconds(20);
#endif
		httpClient.DefaultRequestHeaders.Add("User-Agent", "BG3ModManager");
		foreach(var kvp in headers)
		{
			httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
		}
		DivinityApp.Log($"Opening stream to {downloadUrl}");
		var resp = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
		disposables.Add(resp);
		resp.EnsureSuccessStatusCode();
		var contentStream = await resp.Content.ReadAsStreamAsync(token);
		disposables.Add(contentStream);
		return contentStream;
	}

	private static async Task ProcessDownloadTaskAsync(DownloadTask task, CancellationToken token)
	{
		var disp = new CompositeDisposable();
		try
		{
			task.TimeStarted = DateTimeOffset.Now;
			using var stream = await OpenUriAsStreamAsync(task.DownloadUri, [], disp, token);
			await using var outputStream = new FileStream(task.OutputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 128000, FileOptions.Asynchronous);

			await ReadWriteContentStreamAsync(stream, outputStream, token);
			task.TimeFinished = DateTimeOffset.Now;
			task.Status = DownloadTaskStatus.Finished;
		}
		catch (TaskCanceledException)
		{
			task.Status = DownloadTaskStatus.Cancelled;
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error downloading uri: {ex}");
			task.Error = ex;
			task.Status = DownloadTaskStatus.Error;
		}
		finally
		{
			disp.Dispose();
		}
	}

	public void StartNextDownload()
	{
		if(ActiveDownloads < MaxSimultaneous && _downloadQueue.TryDequeue(out var task))
		{
			task.TaskDisposable = RxApp.TaskpoolScheduler.ScheduleAsync(async (sch, token) =>
			{
				ActiveDownloads += 1;
				await ProcessDownloadTaskAsync(task, token);
				ActiveDownloads -= 1;
			});
		}
	}

	public void AddDownload(string name, Uri downloadUri, string outputFilePath)
	{
		var task = new DownloadTask(Guid.NewGuid(), name, downloadUri, outputFilePath);
		_tasks.AddOrUpdate(task);
		_downloadQueue.Enqueue(task);
	}

	public DownloadManagerService()
	{
		var conn = _tasks.Connect();
		conn.SortAndBind(out _downloads, ByOldestStarted).Subscribe();
	}
}
