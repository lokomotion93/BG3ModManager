namespace ModManager.Services;

public interface IRegistryService
{
	bool IsGOG { get; }

	string GetTruePath(string path);

	/// <summary>
	/// Get a program's installed directory via the Registry.
	/// </summary>
	/// <param name="displayName"></param>
	/// <returns></returns>
	string? GetApplicationInstallPath(string displayName);

	string GetGameInstallPath(string steamGameInstallPath, string steamAppId);
	bool IsAssociatedWithNXMProtocol(string exePath);
	bool SetNXMProtocol(string exePath);
}
