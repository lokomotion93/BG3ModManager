using ModManager.Models.Mod;

namespace ModManager.Models.GitHub;

[DataContract]
public partial class GitHubModData : ReactiveObject
{
	[Reactive]
	[DataMember]
	public partial string? Url { get; set; }

	[Reactive]
	[DataMember]
	public partial GitHubLatestReleaseData LatestRelease { get; set; }

	/// <summary>
	/// True if Url is set.
	/// </summary>
	[Reactive] public partial bool IsEnabled { get; private set; }

	[ObservableAsProperty] public partial string? Author { get; }
	[ObservableAsProperty] public partial string? Repository { get; }

	public void Update(GitHubModData data)
	{
		Url = data.Url;
		if (data.LatestRelease != null)
		{
			LatestRelease.Version = data.LatestRelease.Version;
			LatestRelease.Description = data.LatestRelease.Description;
			LatestRelease.Date = data.LatestRelease.Date;
			LatestRelease.BrowserDownloadLink = data.LatestRelease.BrowserDownloadLink;
		}
	}

	public GitHubModData()
	{
		LatestRelease = new GitHubLatestReleaseData();

		var parseGitHubUrl = this.WhenAnyValue(x => x.Url).Select(ModConfig.GitHubUrlToParts);
		_authorHelper = parseGitHubUrl.Select(x => x.Item1).ToUIPropertyImmediate(this, x => x.Author);
		_repositoryHelper = parseGitHubUrl.Select(x => x.Item2).ToUIPropertyImmediate(this, x => x.Repository);

		this.WhenAnyValue(x => x.Url, url => url.IsValid()).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, x => x.IsEnabled);
	}
}
