using ModManager.Extensions;
using ModManager.Models.Settings;
using ModManager.Services;

namespace ModManager.Models;

public partial class PathwayData : ReactiveObject
{
	/// <summary>
	/// The path to the root game folder, i.e. SteamLibrary\steamapps\common\Baldur's Gate 3
	/// </summary>
	[Reactive] public partial string? InstallPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3
	/// </summary>
	[Reactive] public partial string? AppDataGameFolder { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\Mods
	/// </summary>
	[Reactive] public partial string? AppDataModsPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\Mods_Disabled
	/// </summary>
	[Reactive] public partial string? AppDataInactiveModsPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\PlayerProfiles
	/// </summary>
	[Reactive] public partial string? AppDataProfilesPath { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\ModCrashSanityCheck
	/// </summary>
	[Reactive] public partial string? AppDataModCrashSanityCheck { get; set; }

	/// <summary>
	/// The path to %LOCALAPPDATA%\Larian Studios\Baldur's Gate 3\Script Extender
	/// </summary>
	[Reactive] public partial string? AppDataScriptExtenderPath { get; set; }
	[Reactive] public partial string? LastSaveFilePath { get; set; }
	[Reactive] public partial string? ScriptExtenderLatestReleaseUrl { get; set; }
	[Reactive] public partial string? ScriptExtenderLatestReleaseVersion { get; set; }

	private static readonly IFileSystemService _fs;
	static PathwayData()
	{
		_fs = Locator.Current.GetService<IFileSystemService>()!;
	}

	public void UpdateAppDataPathways(string? appDataGame = null)
	{
		AppDataGameFolder = appDataGame;
		var fs = Locator.Current.GetService<IFileSystemService>();
		if (fs != null && appDataGame.IsValid())
		{
			AppDataModsPath = fs.Path.Join(appDataGame, "Mods");
			AppDataInactiveModsPath = fs.Path.Join(appDataGame, "Mods_Disabled");
			AppDataProfilesPath = fs.Path.Join(appDataGame, "PlayerProfiles");
			AppDataModCrashSanityCheck = fs.Path.Join(appDataGame, "ModCrashSanityCheck");
			AppDataScriptExtenderPath = fs.Path.Join(appDataGame, "Script Extender");
		}
	}

	public static string? ScriptExtenderSettingsFile(ModManagerSettings settings)
	{
		if (settings.GameExecutablePath?.IsExistingFile() == true)
		{
			return _fs.Path.Join(_fs.Path.GetDirectoryName(settings.GameExecutablePath), DivinityApp.EXTENDER_CONFIG_FILE);
		}
		return null;
	}

	public static string? ScriptExtenderUpdaterConfigFile(ModManagerSettings settings)
	{
		if (settings.GameExecutablePath?.IsExistingFile() == true)
		{
			return _fs.Path.Join(_fs.Path.GetDirectoryName(settings.GameExecutablePath), DivinityApp.EXTENDER_UPDATER_CONFIG_FILE);
		}
		return null;
	}
}
