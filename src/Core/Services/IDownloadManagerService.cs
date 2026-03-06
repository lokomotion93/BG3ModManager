using ModManager.Models.App;

namespace ModManager;

public interface IDownloadManagerService
{
	int ActiveDownloads { get; }
	int MaxSimultaneous { get; set; }

	ReadOnlyObservableCollection<DownloadTask> Downloads { get; }

	void AddDownload(DownloadRequest request);
}
