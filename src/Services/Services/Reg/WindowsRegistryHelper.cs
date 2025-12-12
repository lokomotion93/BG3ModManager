using Microsoft.Win32;

namespace ModManager.Services.Reg;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This is only used if OperatingSystem.Windows is true.")]
internal class WindowsRegistryHelper : IRegHelper
{
	const string REG_Steam_32 = @"SOFTWARE\Valve\Steam";
	const string REG_Steam_64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
	const string REG_GOG_32 = @"SOFTWARE\GOG.com\Games";
	const string REG_GOG_64 = @"SOFTWARE\Wow6432Node\GOG.com\Games";

	private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

	const string REG_NXM_PROTOCOL_COMMAND = @"nxm\shell\open\command";

	public object? GetKey(RegistryKey reg, string subKey, string keyValue)
	{
		try
		{
			var key = reg.OpenSubKey(subKey);
			if (key != null)
			{
				return key.GetValue(keyValue);
			}
		}
		catch (Exception e)
		{
			DivinityApp.Log($"Error reading registry subKey ({subKey}): {e}");
		}
		return null;
	}

	public string? GetApplicationInstallPath(string displayName)
	{
		if (!OperatingSystem.IsWindows())
		{
			return null;
		}

		using var rk = Registry.LocalMachine.OpenSubKey(UninstallKey);
		if (rk != null)
		{
			foreach (var skName in rk.GetSubKeyNames())
			{
				using var sk = rk.OpenSubKey(skName);
				if (sk != null && sk.GetValue("DisplayName")?.ToString() == displayName)
				{
					var exeDirectory = sk.GetValue("InstallLocation")?.ToString();
					if (!string.IsNullOrEmpty(exeDirectory))
					{
						return exeDirectory;
					}
				}
			}
		}
		return null;
	}

	public string GetSteamInstallPath()
	{
		var reg = Registry.LocalMachine;
		var installPath = GetKey(reg, REG_Steam_64, "InstallPath");
		if (installPath == null)
		{
			installPath = GetKey(reg, REG_Steam_32, "InstallPath");
		}
		if (installPath != null)
		{
			return (string)installPath;
		}
		return "";
	}

	public string GetGOGInstallPath()
	{
		var reg = Registry.LocalMachine;
		var installPath = GetKey(reg, REG_GOG_32, "path");
		if (installPath == null)
		{
			installPath = GetKey(reg, REG_GOG_64, "path");
		}
		if (installPath != null)
		{
			return (string)installPath;
		}
		return "";
	}

	public bool IsAssociatedWithNXMProtocol(string appExePath)
	{
		//Get the "(Default)" key value
		var shellCommand = GetKey(Registry.ClassesRoot, REG_NXM_PROTOCOL_COMMAND, string.Empty)?.ToString();
		DivinityApp.Log($"{REG_NXM_PROTOCOL_COMMAND}: {shellCommand}");
		if (shellCommand.IsValid())
		{
			return shellCommand.IndexOf(appExePath, StringComparison.OrdinalIgnoreCase) > -1;
		}
		return false;
	}

	public bool SetNXMProtocol(string appExePath)
	{
		try
		{
			var reg = Registry.ClassesRoot;
			var shellCommand = GetKey(Registry.ClassesRoot, REG_NXM_PROTOCOL_COMMAND, string.Empty)?.ToString();
			if (!shellCommand.IsValid())
			{
				var baseKey = reg.CreateSubKey("nxm", true);
				baseKey.SetValue(string.Empty, "URL:NXM Protocol", RegistryValueKind.String);
				baseKey.SetValue("URL Protocol", "", RegistryValueKind.String);
				var shellCommandKey = baseKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command");
				shellCommandKey.SetValue(string.Empty, $"\"{appExePath}\" \"%1\"", RegistryValueKind.String);
				reg.Close();
			}
			else if (!shellCommand.Contains(appExePath, StringComparison.OrdinalIgnoreCase))
			{
				var key = reg.OpenSubKey(REG_NXM_PROTOCOL_COMMAND, true);
				key?.SetValue(string.Empty, $"\"{appExePath}\" \"%1\"", RegistryValueKind.String);
			}
			reg.Close();
			return true;
		}
		catch (Exception ex)
		{
			DivinityApp.Log($"Error updating nxm protocol:\n{ex}");
		}
		return false;
	}
}
