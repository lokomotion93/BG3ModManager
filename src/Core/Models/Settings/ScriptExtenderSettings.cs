using System.ComponentModel;

using ModManager.Locale;
namespace ModManager.Models.Settings;

[DataContract]
public class ScriptExtenderSettings : ReactiveObject, ISerializableSettings
{
	public string FileName => "ScriptExtenderSettings.json";
	public string? GetDirectory() => Locator.Current.GetService<ISettingsService>()?.GetGameExecutableDirectory();
	public bool SkipEmpty => true;

	[Reactive] public bool DevOptionsEnabled { get; set; }
	[Reactive] public bool ExtenderIsAvailable { get; set; }
	[Reactive] public bool ExtenderUpdaterIsAvailable { get; set; }
	[Reactive] public string? ExtenderVersion { get; set; }
	[Reactive] public int ExtenderMajorVersion { get; set; }
	public Version? ModManagerVersion { get; set; }


	[SettingsEntry(nameof(Resources.Settings_Extender_ExportDefaultExtenderSettings), nameof(Resources.Settings_Extender_ExportDefaultExtenderSettings_ToolTip))]
	[DataMember, Reactive]
	[JsonIgnore] // This isn't an actual extender setting, so omit it from the exported json
	[DefaultValue(false)]
	public bool ExportDefaultExtenderSettings { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DeveloperMode), nameof(Resources.Settings_Extender_DeveloperMode_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool DeveloperMode { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_CustomProfile), nameof(Resources.Settings_Extender_CustomProfile_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue("")]
	public string? CustomProfile { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_CreateConsole), nameof(Resources.Settings_Extender_CreateConsole_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool CreateConsole { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_LogFailedCompile), nameof(Resources.Settings_Extender_LogFailedCompile_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool LogFailedCompile { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_EnableLogging), nameof(Resources.Settings_Extender_EnableLogging_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool EnableLogging { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_LogCompile), nameof(Resources.Settings_Extender_LogCompile_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool LogCompile { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_LogDirectory), nameof(Resources.Settings_Extender_LogDirectory_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue("")]
	public string? LogDirectory { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_LogRuntime), nameof(Resources.Settings_Extender_LogRuntime_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool LogRuntime { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DisableLauncher), nameof(Resources.Settings_Extender_DisableLauncher_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool DisableLauncher { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DisableStoryMerge), nameof(Resources.Settings_Extender_DisableStoryMerge_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool DisableStoryMerge { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DisableStoryPatching), nameof(Resources.Settings_Extender_DisableStoryPatching_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool DisableStoryPatching { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ExtendStory), nameof(Resources.Settings_Extender_ExtendStory_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool ExtendStory { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_EnableAchievements), nameof(Resources.Settings_Extender_EnableAchievements_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(true)]
	public bool EnableAchievements { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_SendCrashReports), nameof(Resources.Settings_Extender_SendCrashReports_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(true)]
	public bool SendCrashReports { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_EnableDebugger), nameof(Resources.Settings_Extender_EnableDebugger_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool EnableDebugger { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DebuggerPort), nameof(Resources.Settings_Extender_DebuggerPort_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(9999)]
	public int DebuggerPort { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DebuggerFlags), nameof(Resources.Settings_Extender_DebuggerFlags_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(0)]
	public int DebuggerFlags { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_EnableLuaDebugger), nameof(Resources.Settings_Extender_EnableLuaDebugger_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool EnableLuaDebugger { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_LuaBuiltinResourceDirectory), nameof(Resources.Settings_Extender_LuaBuiltinResourceDirectory_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue("")]
	public string? LuaBuiltinResourceDirectory { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ClearOnReset), nameof(Resources.Settings_Extender_ClearOnReset_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool ClearOnReset { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_DefaultToClientConsole), nameof(Resources.Settings_Extender_DefaultToClientConsole_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool DefaultToClientConsole { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_InsanityCheck), nameof(Resources.Settings_Extender_InsanityCheck_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool InsanityCheck { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_EnablePerfMessages), nameof(Resources.Settings_Extender_EnablePerfMessages_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(true)]
	public bool EnablePerfMessages { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerWarnings), nameof(Resources.Settings_Extender_ProfilerWarnings_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool ProfilerWarnings { get; set; }

	public ScriptExtenderSettings()
	{
		this.SetToDefault();
		ExtenderVersion = string.Empty;
		ExtenderMajorVersion = -1;
	}
}
