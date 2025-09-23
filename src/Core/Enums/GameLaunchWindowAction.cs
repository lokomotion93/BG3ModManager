using ModManager.Locale;

using System.ComponentModel.DataAnnotations;

namespace ModManager;

[JsonConverter(typeof(JsonStringEnumConverter<GameLaunchWindowAction>))]
public enum GameLaunchWindowAction
{
	[Display(Name = nameof(Resources.GameLaunchWindowAction_None), Description = nameof(Resources.GameLaunchWindowAction_None_ToolTip))]
	None,
	[Display(Name = nameof(Resources.GameLaunchWindowAction_Minimize), Description = nameof(Resources.GameLaunchWindowAction_Minimize_ToolTip))]
	Minimize,
	[Display(Name = nameof(Resources.GameLaunchWindowAction_Close), Description = nameof(Resources.GameLaunchWindowAction_Close_ToolTip))]
	Close
}
