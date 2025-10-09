using ModManager.Models.GitHub;
using ModManager.Models.NexusMods;

namespace ModManager.ModUpdater;
public class ModUpdaterResults(Dictionary<string, GitHubLatestReleaseData> github, 
	Dictionary<string, NexusModsModDownloadLink> nexusMods,
	Dictionary<string, ModioFile> modio)
{
	public Dictionary<string, GitHubLatestReleaseData> GitHub { get; } = github;
	public Dictionary<string, NexusModsModDownloadLink> NexusMods { get; } = nexusMods;
	public Dictionary<string, ModioFile> Modio { get; } = modio;
}
