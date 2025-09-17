using DynamicData;

using ModManager.Json;
using ModManager.Models.Mod;

namespace ModManager.Models.Settings;

[DataContract]
public class ModManagerContainerSettings : BaseSettings<ModManagerContainerSettings>, ISerializableSettings
{
	[DataMember]
	[JsonConverter(typeof(DictionaryToSourceCacheConverter<ModContainerSettings>))]
	public SourceCache<ModContainerSettings, string> Containers { get; set; }

	public ModManagerContainerSettings() : base("containers.json")
	{
		Containers = new SourceCache<ModContainerSettings, string>(x => x.Id);
	}
}
