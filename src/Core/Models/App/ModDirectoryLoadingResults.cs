using ModManager.Models.Mod;

namespace ModManager.Models.App;

public class ModDirectoryLoadingResults(params string?[] paths)
{
	public string?[] DirectoryPaths { get; } = paths;
	public Dictionary<string, ModData> Mods { get; init; } = [];
	public List<ModData> Duplicates { get; init; } = [];
}