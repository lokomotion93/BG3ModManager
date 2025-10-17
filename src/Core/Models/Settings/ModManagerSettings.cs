using DynamicData;
using DynamicData.Binding;

using ModManager.Locale;
using ModManager.Util;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace ModManager.Models.Settings;

[DataContract]
public class ModManagerSettings : BaseSettings<ModManagerSettings>, ISerializableSettings, IJsonOnDeserialized
{
	[SettingsEntry(nameof(Resources.Settings_GameDataPath), nameof(Resources.Settings_GameDataPath_ToolTip))]
	[DataMember, Reactive] public string? GameDataPath { get; set; }

	[SettingsEntry(nameof(Resources.Settings_GameExecutablePath), nameof(Resources.Settings_GameExecutablePath_ToolTip))]
	[DataMember, Reactive] public string? GameExecutablePath { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_LaunchDX11), nameof(Resources.Settings_LaunchDX11_ToolTip))]
	[DataMember, Reactive] public bool LaunchDX11 { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_GameStoryLogEnabled), nameof(Resources.Settings_GameStoryLogEnabled_ToolTip))]
	[DataMember, Reactive] public bool GameStoryLogEnabled { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_DisableLauncherTelemetry), nameof(Resources.Settings_DisableLauncherTelemetry_ToolTip))]
	[DataMember, Reactive] public bool DisableLauncherTelemetry { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_DisableLauncherModWarnings), nameof(Resources.Settings_DisableLauncherModWarnings_ToolTip))]
	[DataMember, Reactive] public bool DisableLauncherModWarnings { get; set; }

	[DefaultValue(LaunchGameType.Exe)]
	[SettingsEntry(nameof(Resources.Settings_LaunchType), nameof(Resources.Settings_LaunchType_ToolTip), bindTo: nameof(LaunchTypeIndex))]
	[DataMember, Reactive] public LaunchGameType LaunchType { get; set; }

	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_CustomLaunchAction), nameof(Resources.Settings_CustomLaunchAction_ToolTip), bindVisibilityTo: nameof(IsCustomLaunchEnabled))]
	[DataMember, Reactive] public string? CustomLaunchAction { get; set; }

	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_CustomLaunchArgs), nameof(Resources.Settings_CustomLaunchArgs_ToolTip), bindVisibilityTo: nameof(IsCustomLaunchEnabled))]
	[DataMember, Reactive] public string? CustomLaunchArgs { get; set; }

	[DefaultValue("Orders")]
	[SettingsEntry(nameof(Resources.Settings_LoadOrderPath), nameof(Resources.Settings_LoadOrderPath_ToolTip))]
	[DataMember, Reactive] public string? LoadOrderPath { get; set; }

	[DefaultValue(false)]
	[DataMember, Reactive] public bool LogEnabled { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_AutoAddDependenciesWhenExporting), nameof(Resources.Settings_AutoAddDependenciesWhenExporting_ToolTip))]
	[DataMember, Reactive] public bool AutoAddDependenciesWhenExporting { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_CheckForUpdates), nameof(Resources.Settings_CheckForUpdates_ToolTip))]
	[DataMember, Reactive] public bool CheckForUpdates { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_LimitToSingleInstance), nameof(Resources.Settings_LimitToSingleInstance_ToolTip))]
	[DataMember, Reactive] public bool LimitToSingleInstance { get; set; }

	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_DocumentsFolderPathOverride), nameof(Resources.Settings_DocumentsFolderPathOverride_ToolTip))]
	[DataMember, Reactive] public string? DocumentsFolderPathOverride { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_EnableColorblindSupport), nameof(Resources.Settings_EnableColorblindSupport_ToolTip))]
	[DataMember, Reactive] public bool EnableColorblindSupport { get; set; }

	[DefaultValue(ColorThemeType.BaldursDrip)]
	[SettingsEntry(nameof(Resources.Settings_Theme), nameof(Resources.Settings_Theme_ToolTip), bindTo: nameof(ThemeIndex))]
	[DataMember, Reactive] public ColorThemeType Theme { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_ShiftListFocusOnSwap), nameof(Resources.Settings_ShiftListFocusOnSwap_ToolTip))]
	[DataMember, Reactive] public bool ShiftListFocusOnSwap { get; set; }

	[DefaultValue(GameLaunchWindowAction.None)]
	[SettingsEntry(nameof(Resources.Settings_ActionOnGameLaunch), nameof(Resources.Settings_ActionOnGameLaunch_ToolTip), bindTo: nameof(ActionOnGameLaunchIndex))]
	[DataMember, Reactive]
	public GameLaunchWindowAction ActionOnGameLaunch { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_DisableMissingModWarnings), nameof(Resources.Settings_DisableMissingModWarnings_ToolTip))]
	[DataMember, Reactive] public bool DisableMissingModWarnings { get; set; }

	[DefaultValue(false)]
	[Reactive] public bool DisplayFileNames { get; set; }

	[DefaultValue(false)]
	[Reactive, DataMember] public bool DebugModeEnabled { get; set; }

	[DefaultValue("")]
	[DataMember, Reactive] public string? GameLaunchParams { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_SaveWindowLocation), nameof(Resources.Settings_SaveWindowLocation_ToolTip))]
	[DataMember, Reactive] public bool SaveWindowLocation { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_DeleteModCrashSanityCheck), nameof(Resources.Settings_DeleteModCrashSanityCheck_ToolTip))]
	[DataMember, Reactive] public bool DeleteModCrashSanityCheck { get; set; }

	[DataMember, Reactive] public long LastUpdateCheck { get; set; }
	[DataMember, Reactive] public string? LastOrder { get; set; }
	[DataMember, Reactive] public string? LastImportDirectoryPath { get; set; }
	[DataMember, Reactive] public string? LastLoadedOrderFilePath { get; set; }
	[DataMember, Reactive] public string? LastExtractOutputPath { get; set; }

	[DefaultValue("en")]
	[DataMember, Reactive] public string? Language { get; set; }
	public ObservableCollectionExtended<CultureInfo> Languages { get; }
	[Reactive] public CultureInfo? SelectedLanguage { get; set; }

	[DataMember, Reactive] public ModManagerUpdateSettings UpdateSettings { get; set; }
	[DataMember, Reactive] public WindowSettings Window { get; set; }
	[DataMember, Reactive] public ConfirmationSettings Confirmations { get; set; }

	[Reactive] public bool Loaded { get; set; }
	[Reactive] public bool SettingsWindowIsOpen { get; set; }

	[Reactive] public string? DefaultExtenderLogDirectory { get; set; }
	[Reactive] public string? ExtenderLogDirectory { get; set; }

	[Reactive] public int LaunchTypeIndex { get; set; }
	[Reactive] public int ActionOnGameLaunchIndex { get; set; }
	[Reactive] public int ThemeIndex { get; set; }

	[ObservableAsProperty] public bool IsCustomLaunchEnabled { get; }

	[JsonExtensionData]
	private IDictionary<string, object>? Extras { get; set; }

	void IJsonOnDeserialized.OnDeserialized()
	{
		if(Extras?.Count > 0)
		{
			if (JsonUtils.TryGetExtraProperty(Extras, "LaunchThroughSteam", out bool launchThroughSteam) && launchThroughSteam == true)
			{
				LaunchType = LaunchGameType.Steam;
				Extras.Remove("LaunchThroughSteam");
			}
			if (JsonUtils.TryGetExtraProperty(Extras, "DarkThemeEnabled", out bool? darkThemeEnabled) && darkThemeEnabled == false)
			{
				if(darkThemeEnabled == false)
				{
					Theme = ColorThemeType.Light;
				}
				Extras.Remove("DarkThemeEnabled");
			}
		}
	}

	public ModManagerSettings() : base("settings.json")
	{
		this.SetToDefault();

		UpdateSettings = new();
		Window = new();
		Confirmations = new();

		Languages = [
			CultureInfo.GetCultureInfo("en"),
			CultureInfo.GetCultureInfo("ru"),
			CultureInfo.GetCultureInfo("zh-Hans"),
		];
		//var langs = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(x => x != CultureInfo.InvariantCulture).DistinctBy(x => x.LCID).OrderByDescending(x => x.EnglishName.IndexOf("English") > -1);
		//Languages.AddRange(langs);
	}
}
