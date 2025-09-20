using ModManager.Extensions;
using ModManager.Models.Settings;
using ModManager.Services;

namespace ModManager.Models;

public class PathwayData : ReactiveObject
{
	/// <summary>
	/// The path to the root game folder, i.e. SteamLibrary\steamapps\common\Baldur's Gate 3
	/// </summary>
	[Reactive] public string? InstallPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3
	/// </summary>
	[Reactive] public string? AppDataGameFolder { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\Mods
	/// </summary>
	[Reactive] public string? AppDataModsPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\Mods_Disabled
	/// </summary>
	[Reactive] public string? AppDataInactiveModsPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\PlayerProfiles
	/// </summary>
	[Reactive] public string? AppDataProfilesPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\ModCrashSanityCheck
	/// </summary>
	[Reactive] public string? AppDataModCrashSanityCheck { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\Script Extender
	/// </summary>
	[Reactive] public string? AppDataScriptExtenderPath { get; set; }
	[Reactive] public string? LastSaveFilePath { get; set; }
	[Reactive] public string? ScriptExtenderLatestReleaseUrl { get; set; }
	[Reactive] public string? ScriptExtenderLatestReleaseVersion { get; set; }

	public void UpdateAppDataPathways(string? appDataGame = null)
	{
		AppDataGameFolder = appDataGame;
		var fs = Locator.Current.GetService<IFileSystemService>();
		if (fs != null && appDataGame.IsValid())
		{
			AppDataModsPath = fs.Path.Join(appDataGame, "Mods");
			AppDataInactiveModsPath = fs.Path.Join(appDataGame, "Mods_Disabled");
			AppDataProfilesPath = fs.Path.Join(appDataGame, "Profiles");
			AppDataModCrashSanityCheck = fs.Path.Join(appDataGame, "ModCrashSanityCheck");
			AppDataScriptExtenderPath = fs.Path.Join(appDataGame, "Script Extender");
		}
	}

	public static string? ScriptExtenderSettingsFile(ModManagerSettings settings)
	{
		if (settings.GameExecutablePath?.IsExistingFile() == true)
		{
			return Path.Join(Path.GetDirectoryName(settings.GameExecutablePath), DivinityApp.EXTENDER_CONFIG_FILE);
		}
		return null;
	}

	public static string? ScriptExtenderUpdaterConfigFile(ModManagerSettings settings)
	{
		if (settings.GameExecutablePath?.IsExistingFile() == true)
		{
			return Path.Join(Path.GetDirectoryName(settings.GameExecutablePath), DivinityApp.EXTENDER_UPDATER_CONFIG_FILE);
		}
		return null;
	}
}
