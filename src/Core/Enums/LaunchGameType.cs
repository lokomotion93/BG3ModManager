using System.ComponentModel.DataAnnotations;

using ModManager.Locale;

namespace ModManager;
public enum LaunchGameType
{
	[Display(Name = nameof(Resources.LaunchGameType_Exe), Description = nameof(Resources.LaunchGameType_Exe_ToolTip))]
	Exe,
	[Display(Name = nameof(Resources.LaunchGameType_Steam), Description = nameof(Resources.LaunchGameType_Steam_ToolTip))]
	Steam,
	[Display(Name = nameof(Resources.LaunchGameType_Custom), Description = nameof(Resources.LaunchGameType_Custom_ToolTip))]
	Custom
}