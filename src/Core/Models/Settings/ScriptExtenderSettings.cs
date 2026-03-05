using System.ComponentModel;

using ModManager.Json;
using ModManager.Locale;
using ModManager.Models.Extender;
namespace ModManager.Models.Settings;

[DataContract]
public partial class ScriptExtenderSettings : ReactiveObject, ISerializableSettings
{
	public string FileName => "ScriptExtenderSettings.json";
	public string? GetDirectory() => AppLocator.Current.GetService<ISettingsService>()?.GetGameExecutableDirectory();
	public bool SkipEmpty => true;

	[Reactive] public partial bool DevOptionsEnabled { get; set; }
	[Reactive] public partial bool ExtenderIsAvailable { get; set; }
	[Reactive] public partial bool ExtenderUpdaterIsAvailable { get; set; }
	[Reactive] public partial string? ExtenderVersion { get; set; }
	[Reactive] public partial int ExtenderMajorVersion { get; set; }
	public Version? ModManagerVersion { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ExportDefaultExtenderSettings), nameof(Resources.Settings_Extender_ExportDefaultExtenderSettings_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	[JsonIgnore]
	public partial bool ExportDefaultExtenderSettings { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DeveloperMode), nameof(Resources.Settings_Extender_DeveloperMode_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool DeveloperMode { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_CustomProfile), nameof(Resources.Settings_Extender_CustomProfile_ToolTip))]
	[DataMember]
	[DefaultValue("")]
	public partial string? CustomProfile { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_CreateConsole), nameof(Resources.Settings_Extender_CreateConsole_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool CreateConsole { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogFailedCompile), nameof(Resources.Settings_Extender_LogFailedCompile_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool LogFailedCompile { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableLogging), nameof(Resources.Settings_Extender_EnableLogging_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool EnableLogging { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogCompile), nameof(Resources.Settings_Extender_LogCompile_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool LogCompile { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogDirectory), nameof(Resources.Settings_Extender_LogDirectory_ToolTip))]
	[DataMember]
	[DefaultValue("")]
	public partial string? LogDirectory { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogRuntime), nameof(Resources.Settings_Extender_LogRuntime_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool LogRuntime { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DisableLauncher), nameof(Resources.Settings_Extender_DisableLauncher_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool DisableLauncher { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DisableStoryMerge), nameof(Resources.Settings_Extender_DisableStoryMerge_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool DisableStoryMerge { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DisableStoryPatching), nameof(Resources.Settings_Extender_DisableStoryPatching_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool DisableStoryPatching { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ExtendStory), nameof(Resources.Settings_Extender_ExtendStory_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool ExtendStory { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableAchievements), nameof(Resources.Settings_Extender_EnableAchievements_ToolTip))]
	[DataMember]
	[DefaultValue(true)]
	public partial bool EnableAchievements { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_SendCrashReports), nameof(Resources.Settings_Extender_SendCrashReports_ToolTip))]
	[DataMember]
	[DefaultValue(true)]
	public partial bool SendCrashReports { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableDebugger), nameof(Resources.Settings_Extender_EnableDebugger_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool EnableDebugger { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DebuggerPort), nameof(Resources.Settings_Extender_DebuggerPort_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(9999)]
	public partial int DebuggerPort { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DebuggerFlags), nameof(Resources.Settings_Extender_DebuggerFlags_ToolTip))]
	[DataMember]
	[DefaultValue(0)]
	public partial int DebuggerFlags { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableLuaDebugger), nameof(Resources.Settings_Extender_EnableLuaDebugger_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool EnableLuaDebugger { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LuaBuiltinResourceDirectory), nameof(Resources.Settings_Extender_LuaBuiltinResourceDirectory_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue("")]
	public partial string? LuaBuiltinResourceDirectory { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ClearOnReset), nameof(Resources.Settings_Extender_ClearOnReset_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool ClearOnReset { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DefaultToClientConsole), nameof(Resources.Settings_Extender_DefaultToClientConsole_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool DefaultToClientConsole { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_InsanityCheck), nameof(Resources.Settings_Extender_InsanityCheck_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool InsanityCheck { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_Optick), nameof(Resources.Settings_Extender_Optick_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(true)]
	public partial bool Optick { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnablePerfMessages), nameof(Resources.Settings_Extender_EnablePerfMessages_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(true)]
	public partial bool EnablePerfMessages { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerWarnings), nameof(Resources.Settings_Extender_ProfilerWarnings_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public partial bool ProfilerWarnings { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public partial uint ProfilerLoadThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public partial uint ProfilerLoadThresholdError { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public partial uint ProfilerLoadCallbackThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public partial uint ProfilerLoadCallbackThresholdError { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(1500u)]
	public partial uint ProfilerCallbackThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(5000u)]
	public partial uint ProfilerCallbackThresholdError { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(1000u)]
	public partial uint ProfilerClientCallbackThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(2000u)]
	public partial uint ProfilerClientCallbackThresholdError { get; set; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerLoadThreshold), nameof(Resources.Settings_Extender_ProfilerLoadThreshold_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	public ScriptExtenderProfilerThreshold ProfilerLoadThreshold { get; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerLoadCallbackThreshold), nameof(Resources.Settings_Extender_ProfilerLoadCallbackThreshold_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	public ScriptExtenderProfilerThreshold ProfilerLoadCallbackThreshold { get; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerCallbackThreshold), nameof(Resources.Settings_Extender_ProfilerCallbackThreshold_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	public ScriptExtenderProfilerThreshold ProfilerCallbackThreshold { get; }

	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerClientCallbackThreshold), nameof(Resources.Settings_Extender_ProfilerClientCallbackThreshold_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	public ScriptExtenderProfilerThreshold ProfilerClientCallbackThreshold { get; }

	public ScriptExtenderSettings()
	{
		this.SetToDefault();
		ExtenderVersion = string.Empty;
		ExtenderMajorVersion = -1;

		ProfilerLoadThreshold = new();
		ProfilerLoadCallbackThreshold = new();
		ProfilerCallbackThreshold = new();
		ProfilerClientCallbackThreshold = new();
	}
}