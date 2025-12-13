using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

using Microsoft.Win32;

using ModManager.Services.Reg;
using ModManager.Util;

using System.Runtime.InteropServices;

namespace ModManager.Services;

/// <inheritdoc />
public partial class RegistryService : IRegistryService
{
	const string PATH_Steam_WorkshopFolder = @"steamapps/workshop";
	const string PATH_Steam_LibraryFile = @"steamapps/libraryfolders.vdf";

	private readonly IFileSystemService _fs;

	private readonly IRegHelper? _regHelper;

	public string? GetSteamInstallPath() => _regHelper?.GetSteamInstallPath();

	public string GetTruePath(string path)
	{
		try
		{
			var driveType = FileUtils.GetPathDriveType(path);
			if (driveType == System.IO.DriveType.Fixed)
			{
				if (JunctionPoint.Exists(path))
				{
					var realPath = JunctionPoint.GetTarget(path);
					if (realPath.IsValid())
					{
						return realPath;
					}
				}
			}
			else
			{
				DivinityApp.Log($"Skipping junction check for path '{path}'. Drive type is '{driveType}'.");
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error checking junction point '{path}': {ex}");
		}
		return path;
	}

	public string? GetGoGInstallPath() => _regHelper?.GetGOGInstallPath();

	public string? GetSteamGameInstallPath(string gameFolder, string steamAppId)
	{
		try
		{
			var steamInstallPath = GetSteamInstallPath();
			if (steamInstallPath.IsExistingDirectory())
			{
				var appManifest = _fs.Path.Join(steamInstallPath, "steamapps", $"appmanifest_{steamAppId}.acf");
				if (appManifest.IsExistingFile())
				{
					var manifestData = VdfConvert.Deserialize(_fs.File.ReadAllText(appManifest));
					if (manifestData != null)
					{
						foreach (var prop in manifestData.Value.Children().OfType<VProperty>())
						{
							if (prop.Key == "installdir")
							{
								var installDir = prop.Value?.Value<string>();
								if (installDir.IsValid())
								{
									gameFolder = installDir;
									DivinityApp.Log($"Using appmanifest installDir '{installDir}'");
								}
								break;
							}
						}
					}
				}

				var folder = _fs.Path.Join(steamInstallPath, "steamapps", "common", gameFolder);
				DivinityApp.Log($"Looking for game at '{folder}'.");
				if (_fs.Directory.Exists(folder))
				{
					DivinityApp.Log($"Found game at '{folder}'.");
					return folder;
				}
				else
				{
					var libraryFile = _fs.Path.Join(steamInstallPath, PATH_Steam_LibraryFile);
					DivinityApp.Log($"Game not found. Looking for Steam libraries in file '{libraryFile}'.");
					if (_fs.File.Exists(libraryFile))
					{
						List<string> libraryFolders = [];
						try
						{
							var libraryData = VdfConvert.Deserialize(_fs.File.ReadAllText(libraryFile));
							foreach (VProperty token in libraryData.Value.Children())
							{
								if (token.Key != "TimeNextStatsReport" && token.Key != "ContentStatsID")
								{
									var path = token.Value.Children().Cast<VProperty>().FirstOrDefault(x => x.Key == "path");
									if (path != null && path.Value is VValue innerValue)
									{
										var p = innerValue.Value<string>();
										if (p.IsExistingDirectory())
										{
											DivinityApp.Log($"Found steam library folder at '{p}'.");
											libraryFolders.Add(p);
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							DivinityApp.Log($"Error parsing steam library file at '{libraryFile}': {ex}");
						}

						foreach (var folderPath in libraryFolders)
						{
							var checkFolder = _fs.Path.Join(folderPath, "steamapps", "common", gameFolder);
							if (checkFolder.IsExistingDirectory())
							{
								DivinityApp.Log($"Found game at '{checkFolder}'.");
								return checkFolder;
							}
						}
					}
					else
					{
						DivinityApp.Log($"Steam library not found at '{libraryFile}'");
					}
				}
			}
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"[*ERROR*] Error finding game path: {ex}");
		}
		return null;
	}

	/// <inheritdoc />
	public string? GetApplicationInstallPath(string displayName)
	{
		return _regHelper?.GetApplicationInstallPath(displayName);
	}

	/// <inheritdoc />
	public bool IsAssociatedWithNXMProtocol(string exePath)
	{
		return _regHelper?.IsAssociatedWithNXMProtocol(exePath) == true;
	}

	/// <inheritdoc />
	public bool SetNXMProtocol(string exePath)
	{
		return _regHelper?.SetNXMProtocol(exePath) == true;
	}

	public RegistryService(IFileSystemService fs)
	{
		_fs = fs;

		if (OperatingSystem.IsWindows())
		{
			_regHelper = new WindowsRegistryHelper();
		}
		else if (OperatingSystem.IsLinux())
		{
			_regHelper = new LinuxRegistryHelper(fs);
			//TODO register protocol with x-scheme-handler / mime handler
		}
		else
		{
			_regHelper = null;
		}
	}
}
