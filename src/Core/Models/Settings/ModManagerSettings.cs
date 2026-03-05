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
public partial class ModManagerSettings : BaseSettings<ModManagerSettings>, ISerializableSettings, IJsonOnDeserialized
{

	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_GameDataPath), nameof(Resources.Settings_GameDataPath_ToolTip))]
	public partial string? GameDataPath { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_GameExecutablePath), nameof(Resources.Settings_GameExecutablePath_ToolTip))]
	public partial string? GameExecutablePath { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_LaunchDX11), nameof(Resources.Settings_LaunchDX11_ToolTip))]
	[DefaultValue(false)]
	public partial bool LaunchDX11 { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_GameStoryLogEnabled), nameof(Resources.Settings_GameStoryLogEnabled_ToolTip))]
	[DefaultValue(false)]
	public partial bool GameStoryLogEnabled { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_DisableLauncherTelemetry), nameof(Resources.Settings_DisableLauncherTelemetry_ToolTip))]
	[DefaultValue(false)]
	public partial bool DisableLauncherTelemetry { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_DisableLauncherModWarnings), nameof(Resources.Settings_DisableLauncherModWarnings_ToolTip))]
	[DefaultValue(false)]
	public partial bool DisableLauncherModWarnings { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_LaunchType), nameof(Resources.Settings_LaunchType_ToolTip), bindTo: nameof(LaunchTypeIndex))]
	[DefaultValue(LaunchGameType.Exe)]
	public partial LaunchGameType LaunchType { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_CustomLaunchAction), nameof(Resources.Settings_CustomLaunchAction_ToolTip), bindVisibilityTo: nameof(IsCustomLaunchEnabled))]
	[DefaultValue("")]
	public partial string? CustomLaunchAction { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_CustomLaunchArgs), nameof(Resources.Settings_CustomLaunchArgs_ToolTip), bindVisibilityTo: nameof(IsCustomLaunchEnabled))]
	[DefaultValue("")]
	public partial string? CustomLaunchArgs { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_LoadOrderPath), nameof(Resources.Settings_LoadOrderPath_ToolTip))]
	[DefaultValue("Orders")]
	public partial string? LoadOrderPath { get; set; }

	[DefaultValue(false)]

	[Reactive]
	[DataMember]
	public partial bool LogEnabled { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_AutoAddDependenciesWhenExporting), nameof(Resources.Settings_AutoAddDependenciesWhenExporting_ToolTip))]
	[DefaultValue(true)]
	public partial bool AutoAddDependenciesWhenExporting { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_CheckForUpdates), nameof(Resources.Settings_CheckForUpdates_ToolTip))]
	[DefaultValue(true)]
	public partial bool CheckForUpdates { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_LimitToSingleInstance), nameof(Resources.Settings_LimitToSingleInstance_ToolTip))]
	[DefaultValue(true)]
	public partial bool LimitToSingleInstance { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_DocumentsFolderPathOverride), nameof(Resources.Settings_DocumentsFolderPathOverride_ToolTip))]
	[DefaultValue("")]
	public partial string? DocumentsFolderPathOverride { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_EnableColorblindSupport), nameof(Resources.Settings_EnableColorblindSupport_ToolTip))]
	[DefaultValue(false)]
	public partial bool EnableColorblindSupport { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_Theme), nameof(Resources.Settings_Theme_ToolTip), bindTo: nameof(ThemeIndex))]
	[DefaultValue(ColorThemeType.BaldursDrip)]
	public partial ColorThemeType Theme { get; set; }


	[Reactive]
	[DataMember]
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_ShiftListFocusOnSwap), nameof(Resources.Settings_ShiftListFocusOnSwap_ToolTip))]
	public partial bool ShiftListFocusOnSwap { get; set; }

	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_ActionOnGameLaunch), nameof(Resources.Settings_ActionOnGameLaunch_ToolTip), bindTo: nameof(ActionOnGameLaunchIndex))]
	[DefaultValue(GameLaunchWindowAction.None)]
	public partial GameLaunchWindowAction ActionOnGameLaunch { get; set; }

	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_DisableMissingModWarnings), nameof(Resources.Settings_DisableMissingModWarnings_ToolTip))]
	[DefaultValue(false)]
	public partial bool DisableMissingModWarnings { get; set; }

	[Reactive]
	[DefaultValue(false)]
	public partial bool DisplayFileNames { get; set; }


	[Reactive]
	[DataMember]
	[DefaultValue(false)]
	public partial bool DebugModeEnabled { get; set; }


	[Reactive]
	[DataMember]
	[DefaultValue("")]
	public partial string? GameLaunchParams { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_SaveWindowLocation), nameof(Resources.Settings_SaveWindowLocation_ToolTip))]
	[DefaultValue(false)]
	public partial bool SaveWindowLocation { get; set; }


	[Reactive]
	[DataMember]
	[SettingsEntry(nameof(Resources.Settings_DeleteModCrashSanityCheck), nameof(Resources.Settings_DeleteModCrashSanityCheck_ToolTip))]
	[DefaultValue(true)]
	public partial bool DeleteModCrashSanityCheck { get; set; }


	[Reactive]
	[DataMember]
	public partial long LastUpdateCheck { get; set; }

	[Reactive]
	[DataMember]
	public partial string? LastOrder { get; set; }

	[Reactive]
	[DataMember]
	public partial string? LastImportDirectoryPath { get; set; }

	[Reactive]
	[DataMember]
	public partial string? LastLoadedOrderFilePath { get; set; }

	[Reactive]
	[DataMember]
	public partial string? LastExtractOutputPath { get; set; }


	[Reactive]
	[DataMember]
	[DefaultValue("en")]
	public string? Language { get; set; }
	public ObservableCollectionExtended<CultureInfo> Languages { get; }
	[Reactive] public partial CultureInfo? SelectedLanguage { get; set; }


	[Reactive]
	[DataMember]
	public partial ModManagerUpdateSettings UpdateSettings { get; set; }

	[Reactive]
	[DataMember]
	public partial WindowSettings Window { get; set; }

	[Reactive]
	[DataMember]
	public partial ConfirmationSettings Confirmations { get; set; }

	[Reactive] public partial bool Loaded { get; set; }
	[Reactive] public partial bool SettingsWindowIsOpen { get; set; }

	[Reactive] public partial string? DefaultExtenderLogDirectory { get; set; }
	[Reactive] public partial string? ExtenderLogDirectory { get; set; }

	[Reactive] public partial int LaunchTypeIndex { get; set; }
	[Reactive] public partial int ActionOnGameLaunchIndex { get; set; }
	[Reactive] public partial int ThemeIndex { get; set; }

	[ObservableAsProperty] public partial bool IsCustomLaunchEnabled { get; }

	[JsonExtensionData]
	private IDictionary<string, object>? Extras { get; set; }

	void IJsonOnDeserialized.OnDeserialized()
	{
		if(Extras?.Count > 0)
		{
			if (JsonUtils.TryGetExtraProperty(Extras, "LaunchThroughSteam", out bool? launchThroughSteam))
			{
				if(launchThroughSteam == true)
				{
					LaunchType = LaunchGameType.Steam;
				}
				Extras.Remove("LaunchThroughSteam");
			}
			if (JsonUtils.TryGetExtraProperty(Extras, "DarkThemeEnabled", out bool? darkThemeEnabled))
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

		_isCustomLaunchEnabledHelper = this.WhenAnyValue(x => x.LaunchType, x => x == LaunchGameType.Custom).ToUIPropertyImmediate(this, x => x.IsCustomLaunchEnabled);
		//var langs = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(x => x != CultureInfo.InvariantCulture).DistinctBy(x => x.LCID).OrderByDescending(x => x.EnglishName.IndexOf("English") > -1);
		//Languages.AddRange(langs);
	}
}
