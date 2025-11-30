using ModManager.Models.Mod;

namespace ModManager.Models.Conflicts;

public partial class DivinityConflictEntryData : ReactiveObject
{
	[Reactive] public partial string? Target { get; set; }
	[Reactive] public partial string? Name { get; set; }

	public List<DivinityConflictModData> ConflictModDataList { get; set; } = [];
}

public class DivinityConflictModData(ModData mod, string val = "") : ReactiveObject
{
	public ModData Mod => mod;

	public string? Value { get; set; } = val;
}