using DynamicData;

using ModManager.Models;
using ModManager.Models.App;
using ModManager.Models.Mod;
using ModManager.Models.Settings;
using ModManager.Util;

using ReactiveUI;

using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ModManager.Services;

public class SettingsService : ReactiveObject, ISettingsService
{
	private readonly IFileSystemService _fs;

	public AppSettings AppSettings { get; init; }
	public ModManagerSettings ManagerSettings { get; init; }
	public UserModConfig ModConfig { get; init; }
	public ScriptExtenderSettings ExtenderSettings { get; init; }
	public ScriptExtenderUpdateConfig ExtenderUpdaterSettings { get; init; }
	public ModManagerContainerSettings ContainerSettings { get; init; }
	public InactiveModsConfig InactiveMods { get; init; }

	private readonly List<ISerializableSettings> _loadSettings;
	private readonly List<ISerializableSettings> _saveSettings;

	private readonly Dictionary<string, IDisposable?> _saveTasks = [];

	private void CancelSaveTask(ISerializableSettings? settings = null)
	{
		if(settings != null)
		{
			if (_saveTasks.TryGetValue(settings.FileName, out var disp))
			{
				disp?.Dispose();
				_saveTasks.Remove(settings.FileName);
			}
		}
		else
		{
			foreach(var disp in _saveTasks.Values)
			{
				disp?.Dispose();
			}
			_saveTasks.Clear();
		}
	}

	public bool TryLoadAppSettings(out Exception? error)
	{
		error = null;
		try
		{
			LoadAppSettings();
			return true;
		}
		catch (Exception ex)
		{
			error = ex;
		}
		return false;
	}

	private void LoadAppSettings()
	{
		var resourcesFolder = DivinityApp.GetAppDirectory(DivinityApp.PATH_RESOURCES);
		var appFeaturesPath = Path.Join(resourcesFolder, DivinityApp.PATH_APP_FEATURES);
		var defaultPathwaysPath = Path.Join(resourcesFolder, DivinityApp.PATH_DEFAULT_PATHWAYS);
		var ignoredModsPath = Path.Join(resourcesFolder, DivinityApp.PATH_IGNORED_MODS);

		DivinityApp.Log($"Loading resources from '{resourcesFolder}'");

		if (File.Exists(appFeaturesPath))
		{
			var savedFeatures = JsonUtils.SafeDeserializeFromPath<Dictionary<string, bool>>(appFeaturesPath);
			if (savedFeatures != null)
			{
				var features = new Dictionary<string, bool>(savedFeatures, StringComparer.OrdinalIgnoreCase);
				AppSettings.Features.ApplyDictionary(features);
			}
		}

		if (File.Exists(defaultPathwaysPath))
		{
			AppSettings.DefaultPathways = JsonUtils.SafeDeserializeFromPath<DefaultPathwayData>(defaultPathwaysPath);
		}

		if (File.Exists(ignoredModsPath))
		{
			var ignoredModsData = JsonUtils.SafeDeserializeFromPath<IgnoredModsData>(ignoredModsPath);
			if (ignoredModsData != null)
			{
				if (ignoredModsData.IgnoreBuiltinPath != null)
				{
					foreach (var path in ignoredModsData.IgnoreBuiltinPath)
					{
						if (path.IsValid())
						{
							ModDataLoader.IgnoreBuiltinPath.Add(path.Replace(Path.DirectorySeparatorChar, '/'));
						}
					}
				}

				foreach (var ignoredMod in ignoredModsData.Mods)
				{
					if(ignoredMod.UUID.IsValid())
					{
						var mod = new ModData(ignoredMod.UUID);
						mod.SetIsBaseGameMod(true);
						mod.IsLarianMod = true;
						if (ignoredMod.Name.IsValid()) mod.Name = ignoredMod.Name;
						if (ignoredMod.Description.IsValid()) mod.Description = ignoredMod.Description;
						if (ignoredMod.Folder.IsValid()) mod.Folder = ignoredMod.Folder;
						if (ignoredMod.Type.IsValid()) mod.ModType = ignoredMod.Type;
						if (ignoredMod.Author.IsValid()) mod.Author = ignoredMod.Author;
						if (ignoredMod.Version != null) mod.Version = new LarianVersion(ignoredMod.Version.Value);
						if (ignoredMod.Tags.IsValid()) mod.AddTags(ignoredMod.Tags.Split(';'));

						var existing = DivinityApp.IgnoredMods.Lookup(mod.UUID);
						if (!existing.HasValue || existing.Value.Version < mod.Version)
						{
							DivinityApp.IgnoredMods.AddOrUpdate(mod);
						}
					}
				}

				foreach (var uuid in ignoredModsData.IgnoreDependencies)
				{
					if(uuid.IsValid())
					{
						DivinityApp.IgnoredDependencyMods.Add(uuid);
					}
				}

				if(ignoredModsData.MainCampaign?.IsValid() == true)
				{
					var modManager = Locator.Current.GetService<IModManagerService>();
					if (modManager != null)
					{
						modManager.MainCampaignGuid = ignoredModsData.MainCampaign;
					}
				}

				//DivinityApp.LogMessage("Ignored mods:\n" + string.Join("\n", DivinityApp.IgnoredMods.Select(x => x.Name)));
			}
		}
	}

	public bool TrySaveAll(out List<Exception> errors)
	{
		CancelSaveTask();
		var capturedErrors = new List<Exception>();
		_saveSettings.ForEach(entry =>
		{
			if (!entry.Save(out var ex) && ex != null)
			{
				capturedErrors.Add(ex);
			}
		});
		errors = capturedErrors;
		return errors.Count == 0;
	}

	public bool TryLoadAll(out List<Exception> errors, bool saveIfNotFound = true)
	{
		var capturedErrors = new List<Exception>();
		_loadSettings.ForEach(entry =>
		{
			if (!entry.Load(out var ex, saveIfNotFound) && ex != null)
			{
				capturedErrors.Add(ex);
			}
		});
		errors = capturedErrors;
		return errors.Count == 0;
	}

	public bool TrySave(ISerializableSettings settings, out Exception? ex)
	{
		return settings.Save(out ex);
	}

	public bool TryLoad(ISerializableSettings settings, out Exception? ex, bool saveIfNotFound = true)
	{
		return settings.Load(out ex, saveIfNotFound);
	}

	public void UpdateLastUpdated(IList<string> updatedModIds)
	{
		if (updatedModIds.Count > 0)
		{
			var time = DateTime.Now.Ticks;
			foreach (var id in updatedModIds)
			{
				ModConfig.LastUpdated[id] = time;
			}
			ModConfig.Save(out _);
		}
	}

	public void UpdateLastUpdated(IList<ModData> updatedMods)
	{
		if (updatedMods.Count > 0)
		{
			var time = DateTime.Now.Ticks;
			foreach (var mod in updatedMods)
			{
				if (!string.IsNullOrEmpty(mod.UUID)) ModConfig.LastUpdated[mod.UUID] = time;
			}
			ModConfig.Save(out _);
		}
	}

	public string? GetGameExecutableDirectory()
	{
		var directory = _fs.Path.GetDirectoryName(ManagerSettings.GameExecutablePath);
		if (directory.IsValid())
		{
			return directory;
		}
		var pathways = Locator.Current.GetService<PathwaysService>();
		if(pathways != null && pathways.Data.InstallPath.IsExistingDirectory())
		{
			return _fs.Path.Join(pathways.Data.InstallPath, "bin");
		}
		return null;
	}

	private static string? GetExtenderLogsDirectory(string? defaultDirectory, string? logDirectory)
	{
		if (!logDirectory.IsValid())
		{
			return defaultDirectory;
		}
		return logDirectory;
	}

	public void QueueSave(ISerializableSettings settings, TimeSpan delay)
	{
		var key = settings.FileName;
		CancelSaveTask(settings);
		void saveAction() {
			TrySave(settings, out _);
			_saveTasks.Remove(key);
		};
		var newDisp = RxApp.MainThreadScheduler.Schedule(TimeSpan.FromMilliseconds(250), saveAction);
		_saveTasks[key] = newDisp;
	}

	public SettingsService(IFileSystemService fs)
	{
		_fs = fs;

		AppSettings = new();
		ManagerSettings = new();
		ModConfig = new();
		ExtenderSettings = new();
		ExtenderUpdaterSettings = new();
		ContainerSettings = new();
		InactiveMods = new();

		_loadSettings = [ManagerSettings, ModConfig, ExtenderSettings, ExtenderUpdaterSettings, ContainerSettings, InactiveMods];
		_saveSettings = [ManagerSettings, ModConfig, ExtenderSettings, ExtenderUpdaterSettings, ContainerSettings, InactiveMods];

		var whenDebugMode = ManagerSettings.WhenAnyValue(x => x.DebugModeEnabled);
		var whenDevMode = ExtenderSettings.WhenAnyValue(x => x.DeveloperMode);
		var whenDevOptions = whenDebugMode.CombineLatest(whenDevMode).AnyTrue();
		whenDevOptions.BindTo(ExtenderSettings, x => x.DevOptionsEnabled);
		whenDevOptions.BindTo(ExtenderUpdaterSettings, x => x.DevOptionsEnabled);

		this.WhenAnyValue(x => x.ManagerSettings.DefaultExtenderLogDirectory, x => x.ExtenderSettings.LogDirectory)
		.Select(x => GetExtenderLogsDirectory(x.Item1, x.Item2))
		.BindTo(ManagerSettings, x => x.ExtenderLogDirectory);

		ManagerSettings.WhenAnyValue(x => x.DebugModeEnabled).Subscribe(b => DivinityApp.DeveloperModeEnabled = b);
		ManagerSettings.WhenAnyValue(x => x.LaunchType, launchType => launchType == LaunchGameType.Custom).ToUIProperty(ManagerSettings, x => x.IsCustomLaunchEnabled);

		var settingsWindowIsOpen = ManagerSettings.WhenAnyValue(x => x.SettingsWindowIsOpen);

		//Binding ComboBox selections back to enums

		ManagerSettings.BindEnumToIndex(
			ManagerSettings.WhenAnyValue(x => x.LaunchType),
			ManagerSettings.WhenAnyValue(x => x.LaunchTypeIndex).SkipUntil(settingsWindowIsOpen),
			x => x.LaunchTypeIndex,
			x => x.LaunchType);

		ManagerSettings.BindEnumToIndex(
			ManagerSettings.WhenAnyValue(x => x.ActionOnGameLaunch),
			ManagerSettings.WhenAnyValue(x => x.ActionOnGameLaunchIndex).SkipUntil(settingsWindowIsOpen),
			x => x.ActionOnGameLaunchIndex,
			x => x.ActionOnGameLaunch);

		ManagerSettings.BindEnumToIndex(
			ManagerSettings.WhenAnyValue(x => x.Theme),
			ManagerSettings.WhenAnyValue(x => x.ThemeIndex).SkipUntil(settingsWindowIsOpen),
			x => x.ThemeIndex,
			x => x.Theme);

		ManagerSettings.WhenAnyValue(x => x.SelectedLanguage).SkipUntil(settingsWindowIsOpen).Select(x => x?.Name).BindTo(ManagerSettings, x => x.Language);
		ManagerSettings.WhenAnyValue(x => x.Language).WhereNotNull().ObserveOn(RxApp.MainThreadScheduler).Subscribe(langName =>
		{
			var langService = Locator.Current.GetService<ILocaleService>();
			if(langService != null)
			{
				try
				{
					var lang = CultureInfo.GetCultureInfo(langName);
					ManagerSettings.SelectedLanguage = lang;
					langService.Culture = lang;
				}
				catch(Exception) { }
			}
		});

		ExtenderUpdaterSettings.BindEnumToIndex(
			ExtenderUpdaterSettings.WhenAnyValue(x => x.UpdateChannel),
			ExtenderUpdaterSettings.WhenAnyValue(x => x.UpdateChannelIndex).SkipUntil(settingsWindowIsOpen),
			x => x.UpdateChannelIndex,
			x => x.UpdateChannel);

		ExtenderSettings.WhenAnyValue(x => x.ProfilerLoadThresholdWarn, x => x.ProfilerLoadThresholdError)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(ExtenderSettings.ProfilerLoadThreshold.Update);

		ExtenderSettings.WhenAnyValue(x => x.ProfilerLoadCallbackThresholdWarn, x => x.ProfilerLoadCallbackThresholdError)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(ExtenderSettings.ProfilerLoadCallbackThreshold.Update);

		ExtenderSettings.WhenAnyValue(x => x.ProfilerCallbackThresholdWarn, x => x.ProfilerCallbackThresholdError)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(ExtenderSettings.ProfilerCallbackThreshold.Update);

		ExtenderSettings.WhenAnyValue(x => x.ProfilerClientCallbackThresholdWarn, x => x.ProfilerClientCallbackThresholdError)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Subscribe(ExtenderSettings.ProfilerClientCallbackThreshold.Update);
	}
}
