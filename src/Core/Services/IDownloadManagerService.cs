using ModManager.Models.App;

namespace ModManager;

public interface IDownloadManagerService
{
	int MaxSimultaneous { get; set; }

	ReadOnlyObservableCollection<DownloadTask> Downloads { get; }
}
