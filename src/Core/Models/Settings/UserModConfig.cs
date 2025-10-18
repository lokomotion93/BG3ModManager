using DynamicData;

using ModManager.Json;
using ModManager.Models.Mod;

namespace ModManager.Models.Settings;

[DataContract]
public class UserModConfig : BaseSettings<UserModConfig>, ISerializableSettings
{
	[DataMember]
	[JsonConverter(typeof(DictionaryToSourceCacheConverter<ModConfig>))]
	public SourceCache<ModConfig, string> Mods { get; set; }

	[DataMember] public Dictionary<string, long> LastUpdated { get; set; }

	public static void UpdateMods(object data, object? existing, object? newValue)
	{
		if (existing is SourceCache<ModConfig, string> existingMods && newValue is SourceCache<ModConfig, string> newMods)
		{
			existingMods.Clear();
			existingMods.AddOrUpdate(newMods.Items);
		}
	}

	public UserModConfig() : base("usermodconfig.json")
	{
		Mods = new SourceCache<ModConfig, string>(x => x.Id);
		LastUpdated = [];
	}
}
