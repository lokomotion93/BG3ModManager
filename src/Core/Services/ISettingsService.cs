using ModManager.Models.Mod;
using ModManager.Models.Settings;

namespace ModManager;

public interface ISettingsService
{
	AppSettings AppSettings { get; }
	ModManagerSettings ManagerSettings { get; }
	UserModConfig ModConfig { get; }
	ScriptExtenderSettings ExtenderSettings { get; }
	ScriptExtenderUpdateConfig ExtenderUpdaterSettings { get; }
	ModManagerContainerSettings ContainerSettings { get; }
	InactiveModsConfig InactiveMods { get; }

	bool TrySave(ISerializableSettings settings, out Exception? ex);
	bool TryLoad(ISerializableSettings settings, out Exception? ex, bool saveIfNotFound = true);
	bool TrySaveAll(out List<Exception> errors);
	bool TryLoadAll(out List<Exception> errors, bool saveIfNotFound = true);
	bool TryLoadAppSettings(out Exception? error);
	void UpdateLastUpdated(IList<string> updatedModIds);
	void UpdateLastUpdated(IList<ModData> updatedMods);
	void QueueSave(ISerializableSettings settings, TimeSpan delay);

	string? GetGameExecutableDirectory();
}