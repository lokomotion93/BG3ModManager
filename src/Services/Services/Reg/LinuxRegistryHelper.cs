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

	public string? GetSteamInstallPath()
	{
		var homeFolder = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
		if(homeFolder.IsExistingDirectory())
		{
			var steamPath = fs.Path.Join(homeFolder, "Steam");
			if(steamPath.IsExistingDirectory())
			{
				return steamPath;
			}
		}
		return null;
	}

	public bool IsAssociatedWithNXMProtocol(string appExePath)
	{
		throw new NotImplementedException();
	}

	public bool SetNXMProtocol(string appExePath)
	{
		throw new NotImplementedException();
	}
}
