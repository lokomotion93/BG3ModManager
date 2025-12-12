using ModManager.Models;
using ModManager.Util;

namespace ModManager.Services;
public class PathwaysService(ISettingsService settingsService, IFileSystemService fs, IRegistryService reg) : IPathwaysService
{
	private readonly ISettingsService _settingsService = settingsService;
	private readonly IFileSystemService _fs = fs;
	private readonly IRegistryService _reg = reg;

	public PathwayData Data { get; } = new();

	private string GetAppDataFolder()
	{
		if (OperatingSystem.IsWindows())
		{
			var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
			if (string.IsNullOrEmpty(appDataFolder) || !_fs.Directory.Exists(appDataFolder))
			{
				var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
				if (_fs.Directory.Exists(userFolder))
				{
					appDataFolder = _fs.Path.Join(userFolder, "AppData", "Local");
				}
			}
			else
			{
				appDataFolder = _fs.Path.Join(appDataFolder);
			}
		}
		else if (OperatingSystem.IsLinux())
		{
			var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify) ?? Environment.GetEnvironmentVariable("$XDG_DATA_HOME");
			if (string.IsNullOrEmpty(appDataFolder) || !_fs.Directory.Exists(appDataFolder))
			{
				//$XDG_DATA_HOME/.local/share/Larian Studios
				var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify) ?? Environment.GetEnvironmentVariable("$HOME");
				if (home.IsExistingDirectory())
				{
					appDataFolder = _fs.Path.Join(home, ".local", "share");
				}
				else
				{
					appDataFolder = "~/.local/share";
				}
			}
			return appDataFolder;
		}
		return string.Empty;
	}

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
		if (!string.IsNullOrEmpty(_settingsService.ManagerSettings.DocumentsFolderPathOverride))
		{
			return _settingsService.ManagerSettings.DocumentsFolderPathOverride;
		}
		var appDataFolder = GetAppDataFolder();
		return _fs.Path.Join(appDataFolder, "Larian Studios");
	}

	//TODO Make this work for both DOS2 and BG3
	public bool SetGamePathways(string? currentGameDataPath, string? gameDataFolderOverride = "")
	{
		try
		{
			var localAppDataFolder = GetAppDataFolder();

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

			Data.UpdateAppDataPathways(appDataGameFolder);

			if (!_fs.Directory.Exists(localAppDataFolder))
			{
				Locator.Current.GetService<IGlobalCommandsService>()?.ShowAlert("Failed to find %LOCALAPPDATA% folder - This is weird", AlertType.Danger);
				DivinityApp.Log($"Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify) return a non-existent path?\nResult({Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)})");
			}

			if (string.IsNullOrWhiteSpace(currentGameDataPath) || !_fs.Directory.Exists(currentGameDataPath))
			{
				var defaultPathways = _settingsService.AppSettings.DefaultPathways;
				var installPath = _reg.GetGameInstallPath(defaultPathways.Steam.RootFolderName!, defaultPathways.Steam.AppID!);

				if (!string.IsNullOrEmpty(installPath) && _fs.Directory.Exists(installPath))
				{
					Data.InstallPath = installPath;
					if (!_fs.File.Exists(_settingsService.ManagerSettings.GameExecutablePath))
					{
						var exePath = "";
						if (!_reg.IsGOG)
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
					if (!_reg.IsGOG)
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
