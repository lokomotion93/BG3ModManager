using ModManager.Locale;
using ModManager.Util;

using System.ComponentModel;

namespace ModManager.Models.Settings;

[DataContract]
public class ModManagerUpdateSettings : ReactiveObject
{
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateScriptExtender), nameof(Resources.Settings_UpdateScriptExtender_ToolTip))]
	[DataMember, Reactive] public bool UpdateScriptExtender { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateGitHubMods), nameof(Resources.Settings_UpdateGitHubMods_ToolTip))]
	[DataMember, Reactive] public bool UpdateGitHubMods { get; set; }

	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateNexusMods), nameof(Resources.Settings_UpdateNexusMods_ToolTip))]
	[DataMember, Reactive] public bool UpdateNexusMods { get; set; }

	//TODO: Remove if Larian doesn't add workshop support
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateModioMods), nameof(Resources.Settings_UpdateModioMods_ToolTip))]
	[DataMember, Reactive] public bool UpdateModioMods { get; set; }

	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_NexusModsAPIKey), nameof(Resources.Settings_NexusModsAPIKey_ToolTip))]
	[DataMember, Reactive] public string? NexusModsAPIKey { get; set; }

	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_ModioAPIKey), nameof(Resources.Settings_ModioAPIKey_ToolTip))]
	[DataMember, Reactive] public string? ModioAPIKey { get; set; }

	[DefaultValue(typeof(TimeSpan), "00:30:00")] // 30 minutes
	[SettingsEntry(nameof(Resources.Settings_MinimumUpdateTimePeriod), nameof(Resources.Settings_MinimumUpdateTimePeriod_ToolTip))]
	[DataMember, Reactive] public TimeSpan MinimumUpdateTimePeriod { get; set; }

	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_AllowAdultContent), nameof(Resources.Settings_AllowAdultContent_ToolTip))]
	[DataMember, Reactive] public bool AllowAdultContent { get; set; }

	[Reactive] public bool IsAssociatedWithNXM { get; set; }

	public ModManagerUpdateSettings()
	{
		this.SetToDefault();

		IsAssociatedWithNXM = RegistryHelper.IsAssociatedWithNXMProtocol(DivinityApp.GetExePath());

#if DEBUG
		NexusModsAPIKey = Environment.GetEnvironmentVariable("NEXUSMODS_API_KEY", EnvironmentVariableTarget.User);
#endif
	}
}
