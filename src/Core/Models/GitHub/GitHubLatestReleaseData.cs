namespace ModManager.Models.GitHub;

public partial class GitHubLatestReleaseData : ReactiveObject
{
	[Reactive] public partial string? Version { get; set; }
	[Reactive] public partial string? Description { get; set; }
	[Reactive] public partial DateTimeOffset Date { get; set; }
	[Reactive] public partial string? BrowserDownloadLink { get; set; }
}
