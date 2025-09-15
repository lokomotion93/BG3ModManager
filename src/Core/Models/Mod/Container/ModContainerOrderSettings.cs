using ModManager.Json;

namespace ModManager.Models.Mod.Container;

[DataContract]
public class ModContainerOrderSettings(string id) : IObjectWithId
{
	[DataMember] public string Id { get; set; } = id;
	[DataMember] public int Index { get; set; }
	[DataMember] public List<string> ModOrder { get; set; } = [];

	[JsonConstructor]
	public ModContainerOrderSettings() : this(string.Empty)
	{

	}
}
