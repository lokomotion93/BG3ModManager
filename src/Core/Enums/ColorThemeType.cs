using ModManager.Locale;

using System.ComponentModel.DataAnnotations;

namespace ModManager;
public enum ColorThemeType
{
	[Display(Name = nameof(Resources.Theme_BaldursDrip))]
	BaldursDrip,
	[Display(Name = nameof(Resources.Theme_Dracula))]
	Dracula,
	[Display(Name = nameof(Resources.Theme_Catppuccin_Mocha))]
	Catppuccin_Mocha,
	[Display(Name = nameof(Resources.Theme_Dark))]
	Dark,
	[Display(Name = nameof(Resources.Theme_Light))]
	Light
}