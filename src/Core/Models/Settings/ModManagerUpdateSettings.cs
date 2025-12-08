using ModManager.Locale;
using ModManager.Util;

using System.ComponentModel;

namespace ModManager.Models.Settings;

[DataContract]
public partial class ModManagerUpdateSettings : ReactiveObject
{
	[Reactive]
	[property: DefaultValue(true)]
	[property: SettingsEntry(nameof(Resources.Settings_UpdateScriptExtender), nameof(Resources.Settings_UpdateScriptExtender_ToolTip))]
	[property: DataMember]
	public bool UpdateScriptExtender { get; set; }

	[Reactive]
	[property: DefaultValue(true)]
	[property: SettingsEntry(nameof(Resources.Settings_UpdateGitHubMods), nameof(Resources.Settings_UpdateGitHubMods_ToolTip))]
	[property: DataMember]
	public bool UpdateGitHubMods { get; set; }

	[Reactive]
	[property: DefaultValue(true)]
	[property: SettingsEntry(nameof(Resources.Settings_UpdateNexusMods), nameof(Resources.Settings_UpdateNexusMods_ToolTip))]
	[property: DataMember]
	public bool UpdateNexusMods { get; set; }

	[Reactive]
	[property: DefaultValue(true)]
	[property: SettingsEntry(nameof(Resources.Settings_UpdateModioMods), nameof(Resources.Settings_UpdateModioMods_ToolTip))]
	[property: DataMember]
	public bool UpdateModioMods { get; set; }

	[Reactive]
	[property: DefaultValue("")]
	[property: SettingsEntry(nameof(Resources.Settings_NexusModsAPIKey), nameof(Resources.Settings_NexusModsAPIKey_ToolTip))]
	[property: DataMember]
	public string? NexusModsAPIKey { get; set; }

	[Reactive]
	[property: DefaultValue("")]
	[property: SettingsEntry(nameof(Resources.Settings_ModioAPIKey), nameof(Resources.Settings_ModioAPIKey_ToolTip))]
	[property: DataMember]
	public string? ModioAPIKey { get; set; }

	[Reactive]
	[property: DefaultValue(typeof(TimeSpan), "00:30:00")] // 30 minutes
	[property: SettingsEntry(nameof(Resources.Settings_MinimumUpdateTimePeriod), nameof(Resources.Settings_MinimumUpdateTimePeriod_ToolTip))]
	[property: DataMember]
	public TimeSpan MinimumUpdateTimePeriod { get; set; }

	[Reactive]
	[property: DefaultValue(false)]
	[property: SettingsEntry(nameof(Resources.Settings_AllowAdultContent), nameof(Resources.Settings_AllowAdultContent_ToolTip))]
	[property: DataMember]
	public bool AllowAdultContent { get; set; }

	[Reactive] public partial bool IsAssociatedWithNXM { get; set; }

	public ModManagerUpdateSettings()
	{
		this.SetToDefault();

		IsAssociatedWithNXM = RegistryHelper.IsAssociatedWithNXMProtocol(DivinityApp.GetExePath());

#if DEBUG
		NexusModsAPIKey = Environment.GetEnvironmentVariable("NEXUSMODS_API_KEY", EnvironmentVariableTarget.User);
#endif
	}
}
