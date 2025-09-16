namespace ModManager.Models.Mod.Order;

[DataContract]
public class ModOrderMod : ReactiveObject, IModOrderEntry
{
	[DataMember] public ModEntryType Type { get; init; }

	[Reactive, DataMember] public string Id { get; set; }
	[Reactive, DataMember] public string? Name { get; set; }

	public ModOrderMod(string id)
	{
		Id = id;
		Type = ModEntryType.Mod;
	}

	[JsonConstructor]
	public ModOrderMod() : this(string.Empty)
	{
		Type = ModEntryType.Mod;
	}
}