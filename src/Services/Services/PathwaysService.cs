using ModManager.Models;
using ModManager.Util;

namespace ModManager.Services;
public class PathwaysService(ISettingsService settingsService, IFileSystemService fs, IRegistryService reg) : IPathwaysService
{
	private readonly ISettingsService _settingsService = settingsService;
	private readonly IFileSystemService _fs = fs;
	private readonly IRegistryService _reg = reg;

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
		if (!string.IsNullOrEmpty(_settingsService.ManagerSettings.DocumentsFolderPathOverride))
		{
			return _settingsService.ManagerSettings.DocumentsFolderPathOverride;
		}
		var appDataFolder = _reg.GetAppDataPath();
		return _fs.Path.Join(appDataFolder, "Larian Studios");
	}

	//TODO Make this work for both DOS2 and BG3
	public bool SetGamePathways(string? currentGameDataPath, string? gameDataFolderOverride = "")
	{
		var defaultPathways = _settingsService.AppSettings.DefaultPathways;
		var settings = _settingsService.ManagerSettings;

		try
		{
			string? localAppDataFolder = null;

			var protonFolder = _reg.GetProtonDataPath(defaultPathways.Steam.AppID!);
			if(protonFolder.IsExistingDirectory())
			{
				localAppDataFolder = protonFolder;
			}
			else
			{
				localAppDataFolder = _reg.GetAppDataPath();
			}

			DivinityApp.Log($"Looking for local app data folder at '{localAppDataFolder}'");

			if (string.IsNullOrWhiteSpace(defaultPathways.DocumentsGameFolder))
			{
				defaultPathways.DocumentsGameFolder = "Larian Studios/Baldur's Gate 3";
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
				DivinityApp.Log($"Using override folder for appDataGameFolder: '{gameDataFolderOverride}'");
			}

			appDataGameFolder = appDataGameFolder.Replace("\\", "/");
			Data.UpdateAppDataPathways(appDataGameFolder);

			if (!_fs.Directory.Exists(localAppDataFolder))
			{
				Locator.Current.GetService<IGlobalCommandsService>()?.ShowAlert("Failed to find %LOCALAPPDATA% folder - This is weird", AlertType.Danger);
				DivinityApp.Log($"Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify) return a non-existent path?\nResult({Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)})");
			}

			var currentExePath = settings.GameExecutablePath;
			var shouldFindExe = !currentExePath.IsExistingFile();
			var shouldFindData = !currentGameDataPath.IsExistingDirectory();

			if (shouldFindData || shouldFindExe)
			{
				var installPath = _reg.GetSteamGameInstallPath(defaultPathways.Steam.RootFolderName!, defaultPathways.Steam.AppID!);
				DivinityApp.Log($"Looking for steam game install path at '{installPath}'.");

				if (installPath.IsExistingDirectory())
				{
					Data.InstallPath = installPath;

					if (shouldFindData)
					{
						var gameDataPath = _fs.Path.Join(installPath, defaultPathways.GameDataFolder).Replace("\\", "/");
						DivinityApp.Log($"Looking for data path at '{gameDataPath}'.");
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

					if (shouldFindExe)
					{
						var exePath = _fs.Path.Join(installPath, defaultPathways.Steam.ExePath).Replace("\\", "/");
						DivinityApp.Log($"Looking for exe path at '{exePath}'.");
						if (_fs.File.Exists(exePath))
						{
							settings.GameExecutablePath = exePath;
							DivinityApp.Log($"Exe path set to '{exePath}'.");
						}
						else
						{
							if(OperatingSystem.IsLinux())
							{
								//Newer versions may just have a "bg3" file, no bg3.exe/bg3_dx11.exe
								exePath = _fs.Path.Join(installPath, "bin", "bg3").Replace("\\", "/");
								DivinityApp.Log($"Looking for linux exe path at '{exePath}'.");
								if (_fs.File.Exists(exePath))
								{
									settings.GameExecutablePath = exePath;
									DivinityApp.Log($"Exe path set to '{exePath}'.");
								}
							}
						}
					}
				}
				else
				{
					if (OperatingSystem.IsWindows())
					{
						var gogFolder = _reg.GetGoGInstallPath();
						if (gogFolder.IsExistingDirectory())
						{
							var gogGameFolder = _fs.Path.Join(gogFolder, defaultPathways.GOG.RootFolderName);
							DivinityApp.Log($"Looking for gog game path at '{gogGameFolder}'.");
							if (gogGameFolder.IsExistingDirectory())
							{
								Data.InstallPath = gogGameFolder;

								if (shouldFindExe)
								{
									var gogExePath = _fs.Path.Join(gogGameFolder, defaultPathways.GOG.ExePath);
									DivinityApp.Log($"Looking for gog exe path at '{gogExePath}'.");
									if (gogExePath.IsExistingFile())
									{
										settings.GameExecutablePath = gogExePath.Replace("\\", "/");
										DivinityApp.Log($"Exe path set to GoG install at '{gogExePath}'.");
									}
								}
								if(shouldFindData)
								{
									var gameDataPath = _fs.Path.Join(gogGameFolder, defaultPathways.GameDataFolder).Replace("\\", "/");
									DivinityApp.Log($"Looking for data path at '{gameDataPath}'.");
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
			}
			else
			{
				var installPath = _fs.DirectoryInfo.New(settings.GameDataPath).Parent.Parent.FullName.Replace("\\", "/");
				Data.InstallPath = installPath;
				DivinityApp.Log($"Set install path at '{installPath}'.");
			}

			if (!_fs.Directory.Exists(settings.GameDataPath))
			{
				DivinityApp.Log($"Failed to find game data path at '{settings.GameDataPath}'");
				return false;
			}

			if (!_fs.File.Exists(settings.GameExecutablePath))
			{
				DivinityApp.Log($"Failed to find executable at '{settings.GameExecutablePath}'");
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
