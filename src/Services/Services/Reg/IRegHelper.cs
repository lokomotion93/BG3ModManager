namespace ModManager.Services.Reg;

internal interface IRegHelper
{
	string? GetSteamInstallPath();
	string? GetGOGInstallPath();
	string? GetApplicationInstallPath(string displayName);
	bool IsAssociatedWithNXMProtocol(string exePath);
	bool SetNXMProtocol(string exePath);
}
