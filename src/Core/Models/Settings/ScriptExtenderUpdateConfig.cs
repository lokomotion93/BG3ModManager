using ModManager.Enums.Extender;
using ModManager.Extensions;
using ModManager.Locale;

using System.ComponentModel;
using System.Runtime.Serialization;

namespace ModManager.Models.Settings;

[DataContract]
public partial class ScriptExtenderUpdateConfig : ReactiveObject, ISerializableSettings
{
	public string FileName => "ScriptExtenderUpdaterConfig.json";
	public string? GetDirectory() => Locator.Current.GetService<ISettingsService>()?.GetGameExecutableDirectory();
	public bool SkipEmpty => true;

	[Reactive] public partial bool DevOptionsEnabled { get; set; }
	[Reactive] public partial bool UpdaterIsAvailable { get; set; }
	[Reactive] public partial int UpdaterVersion { get; set; }
	public Version? ModManagerVersion { get; set; }


	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_UpdateChannel), nameof(Resources.Settings_ExtenderUpdater_UpdateChannel_ToolTip), bindTo: nameof(UpdateChannelIndex))]
	[DataMember, Reactive]
	[DefaultValue(ExtenderUpdateChannel.Release)]
	public ExtenderUpdateChannel UpdateChannel { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_TargetVersion), nameof(Resources.Settings_ExtenderUpdater_TargetVersion_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(null)]
	public string? TargetVersion { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_TargetResourceDigest), nameof(Resources.Settings_ExtenderUpdater_TargetResourceDigest_ToolTip), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(null)]
	public string? TargetResourceDigest { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_DisableUpdates), nameof(Resources.Settings_ExtenderUpdater_DisableUpdates_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool DisableUpdates { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_IPv4Only), nameof(Resources.Settings_ExtenderUpdater_IPv4Only_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool IPv4Only { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_Debug), nameof(Resources.Settings_ExtenderUpdater_Debug_ToolTip))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool Debug { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_ManifestURL), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(null)]
	public string? ManifestURL { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_ManifestName), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(null)]
	public string? ManifestName { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_CachePath), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(null)]
	public string? CachePath { get; set; }

	[SettingsEntry(nameof(Resources.Settings_ExtenderUpdater_ValidateSignature), BindVisibilityTo = nameof(DevOptionsEnabled))]
	[DataMember, Reactive]
	[DefaultValue(false)]
	public bool ValidateSignature { get; set; }

	[Reactive] public partial int UpdateChannelIndex { get; set; }

	public ScriptExtenderUpdateConfig()
	{
		this.SetToDefault();
		UpdaterVersion = -1;
	}
}
