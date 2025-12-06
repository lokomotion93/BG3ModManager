using DynamicData;

using ModManager.Json;

namespace ModManager.Models.Mod;

[DataContract]
public partial class ModScriptExtenderConfig : ReactiveObject
{
	[DataMember, Reactive] public partial int RequiredVersion { get; set; }
	[DataMember, Reactive] public partial string? ModTable { get; set; }

	[JsonConverter(typeof(JsonArrayToSourceListConverter<string>))]
	[DataMember] public SourceList<string> FeatureFlags { get; set; }

	public bool Lua => FeatureFlags.Items.Contains("Lua");
	public bool HasAnySettings => RequiredVersion > -1 || TotalFeatureFlags > 0 || ModTable.IsValid();
	public int TotalFeatureFlags => FeatureFlags.Count;

	public ModScriptExtenderConfig()
	{
		RequiredVersion = -1;
		FeatureFlags = new();
	}
}
