using System;
using System.Collections.Generic;
using System.Text;

namespace ModManager.Services.Reg;

internal class LinuxRegistryHelper : IRegHelper
{
	public string? GetApplicationInstallPath(string displayName)
	{
		throw new NotImplementedException();
	}

	public string? GetGOGInstallPath()
	{
		throw new NotImplementedException();
	}

	public string? GetSteamInstallPath()
	{
		throw new NotImplementedException();
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
