using DynamicData;

using ModManager.Json;
using ModManager.Models.Interfaces;

namespace ModManager.Models.Mod.Order;

public class ModOrderContainer(string id) : IModOrderEntry, INested<List<IModOrderEntry>, IModOrderEntry>
{
	public ModEntryType Type { get; init; } = ModEntryType.Container;

	public string Id { get; set; } = id;
	public string? Name { get; set; }
	public ModContainerSettings? Settings { get; set; }

	public List<IModOrderEntry> Children { get; set; } = [];

	[JsonIgnore]
	public int Index { get; set; }

	[JsonConstructor]
	public ModOrderContainer() : this(string.Empty)
	{
		Type = ModEntryType.Container;
	}
}
