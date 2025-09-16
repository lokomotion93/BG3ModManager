using DynamicData;

using ModManager.Json;

namespace ModManager.Models.Mod.Order;

public class ModOrderContainer : ReactiveObject, IModOrderEntry
{
	public ModEntryType Type { get; init; }

	[Reactive] public string Id { get; set; }
	[Reactive] public string? Name { get; set; }

	[JsonConverter(typeof(JsonArrayToSourceListConverter<IModOrderEntry>))]
	public SourceList<IModOrderEntry> Children { get; set; }

	public ModOrderContainer(string id)
	{
		Id = id;
		Children = new();
		Type = ModEntryType.Container;
	}

	[JsonConstructor]
	public ModOrderContainer() : this(string.Empty)
	{
		Type = ModEntryType.Container;
	}
}
