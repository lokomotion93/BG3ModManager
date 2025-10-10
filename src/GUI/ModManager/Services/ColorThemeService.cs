using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

using DynamicData;

using ModManager.Models.View;

using SukiUI;
using SukiUI.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModManager.Locale;

namespace ModManager.Services;
public class ColorThemeService
{
	public SourceCache<ColorThemeEntry, ColorThemeType> Themes { get; } = new(x => x.Id);

	private ColorThemeEntry? _lastTheme;

	//public static ThemeVariant BaldursDrip { get; } = new ThemeVariant("BaldursDrip", ThemeVariant.Dark);
	//public static ThemeVariant Dracula { get; } = new ThemeVariant("Dracula", ThemeVariant.Dark);

	private void ClearLastTheme()
	{
		if (_lastTheme != null && Application.Current != null)
		{
			Application.Current.Resources.MergedDictionaries.Remove(_lastTheme.Resources);
			if (_lastTheme.Style != null) Application.Current.Styles.Remove(_lastTheme.Style);
			_lastTheme = null;
		}
	}

	public void ChangeTheme(ColorThemeType id)
	{
		if(Application.Current != null)
		{
			var themeLookup = Themes.Lookup(id);
			if (themeLookup.HasValue)
			{
				var theme = themeLookup.Value;
				var resources = Application.Current.Resources;

				ClearLastTheme();
				_lastTheme = theme;

				resources.MergedDictionaries.Add(theme.Resources);
				if (theme.Style != null) Application.Current.Styles.Remove(theme.Style);

				SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
				SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
			}
			else if(id == ColorThemeType.Dark)
			{
				ClearLastTheme();
				SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
			}
			else if(id == ColorThemeType.Light)
			{
				ClearLastTheme();
				SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
			}
		}
	}

	public ColorThemeService(ISettingsService settings, ILocaleService locale)
	{
		ColorThemeEntry[] themes = [
			new ColorThemeEntry(ColorThemeType.BaldursDrip, "avares://ModManager/Styling/ColorThemes/BaldursDrip.axaml"),
			new ColorThemeEntry(ColorThemeType.Dracula, "avares://ModManager/Styling/ColorThemes/Dracula.axaml"),
			new ColorThemeEntry(ColorThemeType.Catppuccin_Mocha, "avares://ModManager/Styling/ColorThemes/Catppuccin/Mocha.axaml"),
		];
		Themes.AddOrUpdate(themes);

		settings.ManagerSettings.WhenAnyValue(x => x.Theme)
			.Throttle(TimeSpan.FromMilliseconds(250))
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(ChangeTheme);
	}
}
