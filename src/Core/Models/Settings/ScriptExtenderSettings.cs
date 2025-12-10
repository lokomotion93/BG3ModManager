using System.ComponentModel;

using ModManager.Json;
using ModManager.Locale;
using ModManager.Models.Extender;
namespace ModManager.Models.Settings;

[DataContract]
public partial class ScriptExtenderSettings : ReactiveObject, ISerializableSettings
{
	public string FileName => "ScriptExtenderSettings.json";
	public string? GetDirectory() => Locator.Current.GetService<ISettingsService>()?.GetGameExecutableDirectory();
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
	public bool ExportDefaultExtenderSettings { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DeveloperMode), nameof(Resources.Settings_Extender_DeveloperMode_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool DeveloperMode { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_CustomProfile), nameof(Resources.Settings_Extender_CustomProfile_ToolTip))]
	[DataMember]
	[DefaultValue("")]
	public string? CustomProfile { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_CreateConsole), nameof(Resources.Settings_Extender_CreateConsole_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool CreateConsole { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogFailedCompile), nameof(Resources.Settings_Extender_LogFailedCompile_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool LogFailedCompile { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableLogging), nameof(Resources.Settings_Extender_EnableLogging_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool EnableLogging { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogCompile), nameof(Resources.Settings_Extender_LogCompile_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool LogCompile { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogDirectory), nameof(Resources.Settings_Extender_LogDirectory_ToolTip))]
	[DataMember]
	[DefaultValue("")]
	public string? LogDirectory { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LogRuntime), nameof(Resources.Settings_Extender_LogRuntime_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool LogRuntime { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DisableLauncher), nameof(Resources.Settings_Extender_DisableLauncher_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool DisableLauncher { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DisableStoryMerge), nameof(Resources.Settings_Extender_DisableStoryMerge_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool DisableStoryMerge { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DisableStoryPatching), nameof(Resources.Settings_Extender_DisableStoryPatching_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool DisableStoryPatching { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ExtendStory), nameof(Resources.Settings_Extender_ExtendStory_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool ExtendStory { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableAchievements), nameof(Resources.Settings_Extender_EnableAchievements_ToolTip))]
	[DataMember]
	[DefaultValue(true)]
	public bool EnableAchievements { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_SendCrashReports), nameof(Resources.Settings_Extender_SendCrashReports_ToolTip))]
	[DataMember]
	[DefaultValue(true)]
	public bool SendCrashReports { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableDebugger), nameof(Resources.Settings_Extender_EnableDebugger_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool EnableDebugger { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DebuggerPort), nameof(Resources.Settings_Extender_DebuggerPort_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(9999)]
	public int DebuggerPort { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DebuggerFlags), nameof(Resources.Settings_Extender_DebuggerFlags_ToolTip))]
	[DataMember]
	[DefaultValue(0)]
	public int DebuggerFlags { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnableLuaDebugger), nameof(Resources.Settings_Extender_EnableLuaDebugger_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool EnableLuaDebugger { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_LuaBuiltinResourceDirectory), nameof(Resources.Settings_Extender_LuaBuiltinResourceDirectory_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue("")]
	public string? LuaBuiltinResourceDirectory { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ClearOnReset), nameof(Resources.Settings_Extender_ClearOnReset_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool ClearOnReset { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_DefaultToClientConsole), nameof(Resources.Settings_Extender_DefaultToClientConsole_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool DefaultToClientConsole { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_InsanityCheck), nameof(Resources.Settings_Extender_InsanityCheck_ToolTip))]
	[DataMember]
	[DefaultValue(false)]
	public bool InsanityCheck { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_Optick), nameof(Resources.Settings_Extender_Optick_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(true)]
	public bool Optick { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_EnablePerfMessages), nameof(Resources.Settings_Extender_EnablePerfMessages_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(true)]
	public bool EnablePerfMessages { get; set; }

	[Reactive]
	[SettingsEntry(nameof(Resources.Settings_Extender_ProfilerWarnings), nameof(Resources.Settings_Extender_ProfilerWarnings_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember]
	[DefaultValue(false)]
	public bool ProfilerWarnings { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public uint ProfilerLoadThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public uint ProfilerLoadThresholdError { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public uint ProfilerLoadCallbackThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(50000u)]
	public uint ProfilerLoadCallbackThresholdError { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(1500u)]
	public uint ProfilerCallbackThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(5000u)]
	public uint ProfilerCallbackThresholdError { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(1000u)]
	public uint ProfilerClientCallbackThresholdWarn { get; set; }

	[Reactive]
	[DataMember]
	[DefaultValue(2000u)]
	public uint ProfilerClientCallbackThresholdError { get; set; }

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