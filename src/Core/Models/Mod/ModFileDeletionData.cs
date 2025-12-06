using ModManager.Util;

namespace ModManager.Models.Mod;

public partial class ModFileDeletionData : ReactiveObject
{
	[Reactive] public partial bool IsSelected { get; set; }
	[Reactive] public partial string? FilePath { get; set; }
	[Reactive] public partial string? DisplayName { get; set; }
	[Reactive] public partial string? UUID { get; set; }
	[Reactive] public partial string? Duplicates { get; set; }
	[Reactive] public partial ModEntryType EntryType { get; set; }

	[ObservableAsProperty] public partial string? DisplayFilePath { get; }

	public static ModFileDeletionData? FromModEntry(IModEntry entry, bool isDeletingDuplicates = false, IEnumerable<ModData>? loadedMods = null)
	{
		if (entry.EntryType is ModEntryType.Mod && entry is ModEntry modEntry && modEntry.Data != null)
		{
			var mod = modEntry.Data;
			var data = new ModFileDeletionData { FilePath = mod.FilePath, DisplayName = mod.DisplayName, IsSelected = true, UUID = mod.UUID, EntryType = entry.EntryType };
			if (isDeletingDuplicates && loadedMods != null)
			{
				var duplicatesStr = loadedMods.FirstOrDefault(x => x.UUID == entry.UUID)?.FilePath;
				if (duplicatesStr.IsValid())
				{
					data.Duplicates = duplicatesStr;
				}
			}
			return data;
		}
		else if (entry.EntryType is ModEntryType.Container)
		{
			var data = new ModFileDeletionData { FilePath = string.Empty, DisplayName = entry.DisplayName, IsSelected = true, UUID = entry.UUID, EntryType = entry.EntryType };
			return data;
		}
		return null;
	}

	public ModFileDeletionData()
	{
		_displayFilePathHelper = this.WhenAnyValue(x => x.FilePath).Select(StringUtils.ReplaceSpecialPathways).ToUIProperty(this, x => x.DisplayFilePath);
	}
}
