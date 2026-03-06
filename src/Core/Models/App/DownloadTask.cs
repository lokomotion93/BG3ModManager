namespace ModManager.Models.App;

public enum DownloadTaskStatus
{
	NotStarted,
	Started,
	Downloading,
	Finished,
	Cancelled,
	Error
}

public partial class DownloadTask : ReactiveObject
{
	private readonly Guid _id;
	private readonly string _name;
	private readonly string _outputPath;
	private readonly Uri _downloadUri;

	public Guid Id => _id;
	public string Name => _name;
	public string OutputPath => _outputPath;
	public Uri DownloadUri => _downloadUri;

	public DateTimeOffset TimeCreated { get; } = DateTimeOffset.Now;
	[Reactive] public partial DateTimeOffset TimeStarted { get; set; }
	[Reactive] public partial DateTimeOffset TimeFinished { get; set; }

	[Reactive] public partial double PercentageDone { get; set; }
	[Reactive] public partial DownloadTaskStatus Status { get; set; }
	[Reactive] public partial Exception? Error { get; set; }

	[Reactive] public partial IDisposable? TaskDisposable { get; set; }
	[Reactive] public partial Func<DownloadTask, Task>? TaskCompleteCallback { get; set; }

	[ObservableAsProperty] public partial bool IsDownloading { get; }
	[ObservableAsProperty] public partial bool IsFinished { get; }
	[ObservableAsProperty] public partial bool IsError { get; }

	public DownloadTask(Guid id, DownloadRequest request, string outputFilePath)
	{
		_id = id;
		_name = request.FileName;
		_downloadUri = request.DownloadUri;
		_outputPath = outputFilePath;
		TaskCompleteCallback = request.DownloadCompleteCallback;

		var whenStatus = this.WhenAnyValue(x => x.Status);
		_isDownloadingHelper = whenStatus.Select(x => x == DownloadTaskStatus.Downloading).ToUIProperty(this, x => x.IsDownloading);
		_isFinishedHelper = whenStatus.Select(x => x == DownloadTaskStatus.Finished).ToUIProperty(this, x => x.IsFinished);
		_isErrorHelper = whenStatus.Select(x => x == DownloadTaskStatus.Error).ToUIProperty(this, x => x.IsError);
	}
}
