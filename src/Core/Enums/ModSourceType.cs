using ModManager.Locale;

using System.ComponentModel.DataAnnotations;

namespace ModManager;

public enum ModSourceType
{
	[Display(Name = nameof(Resources.ModSourceType_None), Description = nameof(Resources.ModSourceType_None_ToolTip))]
	NONE,
	[Display(Name = nameof(Resources.ModSourceType_GitHub), Description = nameof(Resources.ModSourceType_GitHub_ToolTip))]
	GITHUB,
	[Display(Name = nameof(Resources.ModSourceType_NexusMods), Description = nameof(Resources.ModSourceType_NexusMods_ToolTip))]
	NEXUSMODS,
	[Display(Name = nameof(Resources.ModSourceType_Modio), Description = nameof(Resources.ModSourceType_Modio_ToolTip))]
	MODIO
}
