using System.IO.Abstractions;

namespace ModManager.Util;

public class TempFile : IAsyncDisposable
{
	private readonly System.IO.FileStream _stream;
	private readonly string _path;
	private readonly string _sourcePath;

	private readonly int _bufferSize;

	public System.IO.FileStream Stream => _stream;
	public string FilePath => _path;
	public string SourceFilePath => _sourcePath;

	private readonly IFileSystemService _fs;

	//128 KB since we're using asynchronous streams, default is 4 KB
	private TempFile(string sourcePath, int bufferSize = 128000)
	{
		_fs = Locator.Current.GetService<IFileSystemService>()!;

		_bufferSize = bufferSize;
		var tempDir = DivinityApp.GetAppDirectory("Temp");
		_fs.Directory.CreateDirectory(tempDir);
		_path = _fs.Path.Join(tempDir, _fs.Path.GetFileName(sourcePath));
		_sourcePath = sourcePath;
		//_stream = File.Create(_path, _bufferSize, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
		_stream = new System.IO.FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read, _bufferSize, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
	}

	public static async Task<TempFile> CreateAsync(string sourcePath, CancellationToken token)
	{
		var temp = new TempFile(sourcePath);
		await temp.CopyAsync(token);
		return temp;
	}

	public static async Task<TempFile> CreateAsync(string sourcePath, Stream sourceStream, CancellationToken token)
	{
		var temp = new TempFile(sourcePath);
		await temp.CopyAsync(sourceStream, token);
		return temp;
	}

	private async Task CopyAsync(CancellationToken token)
	{
		await using var sourceStream = _fs.FileStream.New(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
		await sourceStream.CopyToAsync(_stream, _bufferSize, token);
	}

	private async Task CopyAsync(Stream sourceStream, CancellationToken token)
	{
		await sourceStream.CopyToAsync(_stream, _bufferSize, token);
	}

	public async ValueTask DisposeAsync()
	{
		if(_stream is not null)
		{
			await _stream.DisposeAsync().ConfigureAwait(false);
		}

		GC.SuppressFinalize(this);
	}
}
