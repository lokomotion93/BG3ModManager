using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.Services.Reg;

internal class LinuxRegistryHelper(IFileSystemService fs) : IRegHelper
{
	public string? GetApplicationInstallPath(string displayName)
	{
		throw new NotImplementedException();
	}

	public string? GetGOGInstallPath()
	{
		return null;
	}

	public string? GetAppDataPath()
	{
		var homeDataFolder = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
		var homeFolder = Environment.GetEnvironmentVariable("HOME");
		DivinityApp.Log($"Checking for 'XDG_DATA_HOME' environment variable: '{homeDataFolder}'");
		if (!homeDataFolder.IsExistingDirectory())
		{
			if (homeFolder.IsExistingDirectory())
			{
				homeDataFolder = fs.Path.Join(homeFolder, ".local", "share");
			}
			else
			{
				DivinityApp.Log("'XDG_DATA_HOME' not set. Trying '~/.local/share'");
				homeDataFolder = "~/.local/share";
			}
		}
		return homeDataFolder;
	}

	public string? GetSteamInstallPath()
	{
		var homeDataFolder = GetAppDataPath();
		if(homeDataFolder.IsExistingDirectory())
		{
			var steamPath = fs.Path.Join(homeDataFolder, "Steam");
			if(steamPath.IsExistingDirectory())
			{
				DivinityApp.Log($"Found steam at {steamPath}");
				return steamPath;
			}
			else
			{
				DivinityApp.Log($"Failed to find steam folder at '{steamPath}'");
			}
		}
		else
		{
			DivinityApp.Log($"Failed to find home data folder at '{homeDataFolder}'");
		}
		return null;
	}

	public bool IsAssociatedWithNXMProtocol(string appExePath)
	{
		return false;
	}

	public bool SetNXMProtocol(string appExePath)
	{
		return false;
	}
}
