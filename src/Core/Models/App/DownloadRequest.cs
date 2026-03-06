namespace ModManager.Models.App;

public record struct DownloadRequest(string FileName, Uri DownloadUri, Func<DownloadTask, Task>? DownloadCompleteCallback = null);
