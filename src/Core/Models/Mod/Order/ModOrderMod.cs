namespace ModManager.Models.Mod.Order;

public class ModOrderMod(string id) : IModOrderEntry
{
	public ModEntryType Type { get; init; } = ModEntryType.Mod;

	public string Id { get; set; } = id;
	public string? Name { get; set; }

	[JsonConstructor]
	public ModOrderMod() : this(string.Empty)
	{
		Type = ModEntryType.Mod;
	}
}