using ModManager.Locale;
using ModManager.Services;
using ModManager.Util;

using System.ComponentModel;

namespace ModManager.Models.Settings;

[DataContract]
public partial class ModManagerUpdateSettings : ReactiveObject
{
	[Reactive]
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateScriptExtender), nameof(Resources.Settings_UpdateScriptExtender_ToolTip))]
	[DataMember]
	public partial bool UpdateScriptExtender { get; set; }

	[Reactive]
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateGitHubMods), nameof(Resources.Settings_UpdateGitHubMods_ToolTip))]
	[DataMember]
	public partial bool UpdateGitHubMods { get; set; }

	[Reactive]
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateNexusMods), nameof(Resources.Settings_UpdateNexusMods_ToolTip))]
	[DataMember]
	public partial bool UpdateNexusMods { get; set; }

	[Reactive]
	[DefaultValue(true)]
	[SettingsEntry(nameof(Resources.Settings_UpdateModioMods), nameof(Resources.Settings_UpdateModioMods_ToolTip))]
	[DataMember]
	public partial bool UpdateModioMods { get; set; }

	[Reactive]
	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_NexusModsAPIKey), nameof(Resources.Settings_NexusModsAPIKey_ToolTip))]
	[DataMember]
	public partial string? NexusModsAPIKey { get; set; }

	[Reactive]
	[DefaultValue("")]
	[SettingsEntry(nameof(Resources.Settings_ModioAPIKey), nameof(Resources.Settings_ModioAPIKey_ToolTip))]
	[DataMember]
	public partial string? ModioAPIKey { get; set; }

	[Reactive]
	[DefaultValue(typeof(TimeSpan), "00:30:00")] // 30 minutes
	[SettingsEntry(nameof(Resources.Settings_MinimumUpdateTimePeriod), nameof(Resources.Settings_MinimumUpdateTimePeriod_ToolTip))]
	[DataMember]
	public partial TimeSpan MinimumUpdateTimePeriod { get; set; }

	[Reactive]
	[DefaultValue(false)]
	[SettingsEntry(nameof(Resources.Settings_AllowAdultContent), nameof(Resources.Settings_AllowAdultContent_ToolTip))]
	[DataMember]
	public partial bool AllowAdultContent { get; set; }

	[Reactive] public partial bool IsAssociatedWithNXM { get; set; }

	[Reactive, DataMember] public partial long LastScriptExtender { get; set; }
	[Reactive, DataMember] public partial long LastGitHubCheck { get; set; }
	[Reactive, DataMember] public partial long LastNexusModsCheck { get; set; }
	[Reactive, DataMember] public partial long LastModioCheck { get; set; }

	public ModManagerUpdateSettings()
	{
		this.SetToDefault();

#if DEBUG
		NexusModsAPIKey = Environment.GetEnvironmentVariable("NEXUSMODS_API_KEY", EnvironmentVariableTarget.User);
#endif
	}
}
