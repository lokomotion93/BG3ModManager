using DynamicData;

using ModManager.Json;

namespace ModManager.Models.Mod;

[DataContract]
public partial class ModScriptExtenderConfig : ReactiveObject
{
	[DataMember, Reactive] public int RequiredVersion { get; set; }
	[DataMember, Reactive] public string? ModTable { get; set; }

	[JsonConverter(typeof(JsonArrayToSourceListConverter<string>))]
	[DataMember] public SourceList<string> FeatureFlags { get; set; }

	[ObservableAsProperty] public partial int TotalFeatureFlags { get; }
	[ObservableAsProperty] public partial bool HasAnySettings { get; }

	public bool Lua => FeatureFlags.Items.Contains("Lua");

	public ModScriptExtenderConfig()
	{
		RequiredVersion = -1;
		FeatureFlags = new();
		_totalFeatureFlagsHelper = FeatureFlags.CountChanged.ToProperty(this, x => x.TotalFeatureFlags);

		_hasAnySettingsHelper = this.WhenAnyValue(x => x.RequiredVersion, x => x.TotalFeatureFlags, x => x.ModTable)
		.Select(x => x.Item1 > -1 || x.Item2 > 0 || x.Item3.IsValid())
		.ToProperty(this, x => x.HasAnySettings);
	}
}
