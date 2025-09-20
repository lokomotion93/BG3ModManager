using ModManager.Models;
using ModManager.Util;

namespace ModManager.Services;
public class PathwaysService(ISettingsService settingsService, IFileSystemService fs) : IPathwaysService
{
	private readonly ISettingsService _settingsService = settingsService;
	private readonly IFileSystemService _fs = fs;

	public PathwayData Data { get; } = new();

	public string GetLarianStudiosAppDataFolder()
	{
		if (_fs.Directory.Exists(Data.AppDataGameFolder))
		{
			var parentDir = _fs.Directory.GetParent(Data.AppDataGameFolder);
			if (parentDir != null)
			{
				return parentDir.FullName;
			}
		}
		string appDataFolder;
		if (!string.IsNullOrEmpty(_settingsService.ManagerSettings.DocumentsFolderPathOverride))
		{
			appDataFolder = _settingsService.ManagerSettings.DocumentsFolderPathOverride;
		}
		else
		{
			appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
			if (string.IsNullOrEmpty(appDataFolder) || !_fs.Directory.Exists(appDataFolder))
			{
				var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
				if (_fs.Directory.Exists(userFolder))
				{
					appDataFolder = _fs.Path.Join(userFolder, "AppData", "Local", "Larian Studios");
				}
			}
			else
			{
				appDataFolder = _fs.Path.Join(appDataFolder, "Larian Studios");
			}
		}
		return appDataFolder;
	}

	//TODO Make this work for both DOS2 and BG3
	public bool SetGamePathways(string currentGameDataPath, string gameDataFolderOverride = "")
	{
		try
		{
			var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);

			if (string.IsNullOrWhiteSpace(_settingsService.AppSettings.DefaultPathways.DocumentsGameFolder))
			{
				_settingsService.AppSettings.DefaultPathways.DocumentsGameFolder = "Larian Studios\\Baldur's Gate 3";
			}

			var appDataGameFolder = _fs.Path.Join(localAppDataFolder, _settingsService.AppSettings.DefaultPathways.DocumentsGameFolder);

			if (!string.IsNullOrEmpty(gameDataFolderOverride) && _fs.Directory.Exists(gameDataFolderOverride))
			{
				appDataGameFolder = gameDataFolderOverride;
				var parentDir = _fs.Directory.GetParent(appDataGameFolder);
				if (parentDir != null)
				{
					localAppDataFolder = parentDir.FullName;
				}
			}
			else if (!_fs.Directory.Exists(appDataGameFolder))
			{
				var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
				if (_fs.Directory.Exists(userFolder))
				{
					localAppDataFolder = _fs.Path.Join(userFolder, "AppData", "Local");
					appDataGameFolder = _fs.Path.Join(localAppDataFolder, _settingsService.AppSettings.DefaultPathways.DocumentsGameFolder);
				}
			}

			Data.UpdateAppDataPathways(appDataGameFolder);

			if (_fs.Directory.Exists(localAppDataFolder))
			{
				_fs.Directory.CreateDirectory(appDataGameFolder);
				DivinityApp.Log($"Larian documents folder set to '{appDataGameFolder}'.");

				if (!_fs.Directory.Exists(Data.AppDataModsPath) && Data.AppDataModsPath.IsValid())
				{
					DivinityApp.Log($"No mods folder found at '{Data.AppDataModsPath}'. Creating folder.");
					_fs.Directory.CreateDirectory(Data.AppDataModsPath);
				}

#if DOS2
				if (!_fs.Directory.Exists(gmCampaignsFolder))
				{
					DivinityApp.Log($"No GM campaigns folder found at '{gmCampaignsFolder}'. Creating folder.");
					_fs.Directory.CreateDirectory(gmCampaignsFolder);
				}
#endif

				if (!_fs.Directory.Exists(Data.AppDataProfilesPath) && Data.AppDataProfilesPath.IsValid())
				{
					DivinityApp.Log($"No PlayerProfiles folder found at '{Data.AppDataProfilesPath}'. Creating folder.");
					_fs.Directory.CreateDirectory(Data.AppDataProfilesPath);
				}
			}
			else
			{
				Locator.Current.GetService<IGlobalCommandsService>()?.ShowAlert("Failed to find %LOCALAPPDATA% folder - This is weird", AlertType.Danger);
				DivinityApp.Log($"Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify) return a non-existent path?\nResult({Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify)})");
			}

			if (string.IsNullOrWhiteSpace(currentGameDataPath) || !_fs.Directory.Exists(currentGameDataPath))
			{
				var defaultPathways = _settingsService.AppSettings.DefaultPathways;
				var installPath = RegistryHelper.GetGameInstallPath(defaultPathways.Steam.RootFolderName,
					defaultPathways.GOG.Registry_32, defaultPathways.GOG.Registry_64, defaultPathways.Steam.AppID);

				if (!string.IsNullOrEmpty(installPath) && _fs.Directory.Exists(installPath))
				{
					Data.InstallPath = installPath;
					if (!_fs.File.Exists(_settingsService.ManagerSettings.GameExecutablePath))
					{
						var exePath = "";
						if (!RegistryHelper.IsGOG)
						{
							exePath = _fs.Path.Join(installPath, _settingsService.AppSettings.DefaultPathways.Steam.ExePath);
						}
						else
						{
							exePath = _fs.Path.Join(installPath, _settingsService.AppSettings.DefaultPathways.GOG.ExePath);
						}
						if (_fs.File.Exists(exePath))
						{
							_settingsService.ManagerSettings.GameExecutablePath = exePath.Replace("\\", "/");
							DivinityApp.Log($"Exe path set to '{exePath}'.");
						}
					}

					var gameDataPath = _fs.Path.Join(installPath, _settingsService.AppSettings.DefaultPathways.GameDataFolder).Replace("\\", "/");
					if (_fs.Directory.Exists(gameDataPath))
					{
						DivinityApp.Log($"Set game data path to '{gameDataPath}'.");
						_settingsService.ManagerSettings.GameDataPath = gameDataPath;
					}
					else
					{
						DivinityApp.Log($"Failed to find game data path at '{gameDataPath}'.");
					}
				}
			}
			else
			{
				var installPath = _fs.Path.GetFullPath(_fs.Path.Join(_settingsService.ManagerSettings.GameDataPath, @"..\..\"));
				Data.InstallPath = installPath;
				if (!_fs.File.Exists(_settingsService.ManagerSettings.GameExecutablePath))
				{
					var exePath = "";
					if (!RegistryHelper.IsGOG)
					{
						exePath = _fs.Path.Join(installPath, _settingsService.AppSettings.DefaultPathways.Steam.ExePath);
					}
					else
					{
						exePath = _fs.Path.Join(installPath, _settingsService.AppSettings.DefaultPathways.GOG.ExePath);
					}
					if (_fs.File.Exists(exePath))
					{
						_settingsService.ManagerSettings.GameExecutablePath = exePath.Replace("\\", "/");
						DivinityApp.Log($"Exe path set to '{exePath}'.");
					}
				}
			}


			if (!_fs.Directory.Exists(_settingsService.ManagerSettings.GameDataPath) || !_fs.File.Exists(_settingsService.ManagerSettings.GameExecutablePath))
			{
				DivinityApp.Log($"Failed to find game data path at '{_settingsService.ManagerSettings.GameDataPath}', and/or executable at '{_settingsService.ManagerSettings.GameExecutablePath}'");
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error setting up game pathways: {ex}");
		}

		return false;
	}
}
