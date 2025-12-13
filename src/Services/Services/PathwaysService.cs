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
			var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify) ?? Environment.GetEnvironmentVariable("XDG_DATA_HOME");
			if (string.IsNullOrEmpty(appDataFolder) || !_fs.Directory.Exists(appDataFolder))
			{
				//$XDG_DATA_HOME/.local/share/Larian Studios
				var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify) ?? Environment.GetEnvironmentVariable("HOME");
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

			var defaultPathways = _settingsService.AppSettings.DefaultPathways;
			var settings = _settingsService.ManagerSettings;

			if (string.IsNullOrWhiteSpace(defaultPathways.DocumentsGameFolder))
			{
				defaultPathways.DocumentsGameFolder = "Larian Studios\\Baldur's Gate 3";
			}

			var appDataGameFolder = _fs.Path.Join(localAppDataFolder, defaultPathways.DocumentsGameFolder);

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
				if (OperatingSystem.IsWindows())
				{
					var installPath = _reg.GetSteamGameInstallPath(defaultPathways.Steam.RootFolderName!, defaultPathways.Steam.AppID!);

					if (installPath.IsExistingDirectory())
					{
						Data.InstallPath = installPath;
						if (!_fs.File.Exists(settings.GameExecutablePath))
						{
							var exePath = _fs.Path.Join(installPath, defaultPathways.Steam.ExePath);
							if (_fs.File.Exists(exePath))
							{
								settings.GameExecutablePath = exePath.Replace("\\", "/");
								DivinityApp.Log($"Exe path set to '{exePath}'.");
							}
						}

						var gameDataPath = _fs.Path.Join(installPath, defaultPathways.GameDataFolder).Replace("\\", "/");
						if (_fs.Directory.Exists(gameDataPath))
						{
							DivinityApp.Log($"Set game data path to '{gameDataPath}'.");
							settings.GameDataPath = gameDataPath;
						}
						else
						{
							DivinityApp.Log($"Failed to find game data path at '{gameDataPath}'.");
						}
					}
					else
					{
						var gogFolder = _reg.GetGoGInstallPath();
						if (gogFolder.IsExistingDirectory())
						{
							var gogGameFolder = _fs.Path.Join(gogFolder, defaultPathways.GOG.RootFolderName);
							if (gogGameFolder.IsExistingDirectory())
							{
								var gogExePath = _fs.Path.Join(gogGameFolder, defaultPathways.GOG.ExePath);
								if (gogExePath.IsExistingFile())
								{
									settings.GameExecutablePath = gogExePath.Replace("\\", "/");
									DivinityApp.Log($"Exe path set to GoG install at '{gogExePath}'.");

									var gameDataPath = _fs.Path.Join(gogGameFolder, defaultPathways.GameDataFolder).Replace("\\", "/");
									if (_fs.Directory.Exists(gameDataPath))
									{
										DivinityApp.Log($"Set game data path to '{gameDataPath}'.");
										settings.GameDataPath = gameDataPath;
									}
									else
									{
										DivinityApp.Log($"Failed to find game data path at '{gameDataPath}'.");
									}
								}
							}
						}
					}
				}
				else if(OperatingSystem.IsLinux())
				{
					///steamapps/compatdata/1086940/pfx/
					var steamInstall = _reg.GetSteamInstallPath();
					if(steamInstall.IsExistingDirectory())
					{
						var gameFolder = _fs.Path.Join(steamInstall, $"steamapps/compatdata/{defaultPathways.Steam.AppID ?? "1086940"}");
						if(gameFolder.IsExistingDirectory())
						{
							Data.InstallPath = gameFolder;
							if (!_fs.File.Exists(settings.GameExecutablePath))
							{
								var exePath = _fs.Path.Join(gameFolder, defaultPathways.Steam.ExePath);
								if (_fs.File.Exists(exePath))
								{
									settings.GameExecutablePath = exePath.Replace("\\", "/");
									DivinityApp.Log($"Exe path set to '{exePath}'.");
								}
							}

							var gameDataPath = _fs.Path.Join(gameFolder, defaultPathways.GameDataFolder).Replace("\\", "/");
							if (_fs.Directory.Exists(gameDataPath))
							{
								DivinityApp.Log($"Set game data path to '{gameDataPath}'.");
								settings.GameDataPath = gameDataPath;
							}
							else
							{
								DivinityApp.Log($"Failed to find game data path at '{gameDataPath}'.");
							}
						}
					}
				}
			}
			else
			{
				var installPath = _fs.Path.GetFullPath(_fs.Path.Join(settings.GameDataPath, @"..\..\"));
				Data.InstallPath = installPath;
				if (!_fs.File.Exists(settings.GameExecutablePath))
				{
					var exePath = _fs.Path.Join(installPath, defaultPathways.Steam.ExePath);
					if (_fs.File.Exists(exePath))
					{
						settings.GameExecutablePath = exePath.Replace("\\", "/");
						DivinityApp.Log($"Exe path set to '{exePath}'.");
					}
				}
			}


			if (!_fs.Directory.Exists(settings.GameDataPath) || !_fs.File.Exists(settings.GameExecutablePath))
			{
				DivinityApp.Log($"Failed to find game data path at '{settings.GameDataPath}', and/or executable at '{settings.GameExecutablePath}'");
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
