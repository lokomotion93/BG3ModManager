using ModManager.Json;

namespace ModManager.Models.Mod.Order;

[JsonConverter(typeof(IModOrderEntryConverter))]
public interface IModOrderEntry
{
	string Id { get; }
	string? Name { get; }
	ModEntryType Type { get; }
}
