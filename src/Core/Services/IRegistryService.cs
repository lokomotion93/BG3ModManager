namespace ModManager.Services;

public interface IRegistryService
{
	string GetTruePath(string path);

	/// <summary>
	/// Get a program's installed directory via the Registry.
	/// </summary>
	/// <param name="displayName"></param>
	/// <returns></returns>
	string? GetApplicationInstallPath(string displayName);

	string? GetAppDataPath();
	string? GetSteamInstallPath();
	string? GetProtonDataPath(string steamAppId);
	string? GetSteamGameInstallPath(string gameFolder, string steamAppId);
	string? GetGoGInstallPath();
	bool IsAssociatedWithNXMProtocol(string exePath);
	bool SetNXMProtocol(string exePath);
}
