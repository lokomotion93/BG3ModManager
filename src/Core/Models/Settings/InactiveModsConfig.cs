using ModManager.Models.Mod.Order;

namespace ModManager.Models.Settings;

[DataContract]
public class InactiveModsConfig : BaseSettings<UserModConfig>, ISerializableSettings
{
	[DataMember]
	public ModOrder Order { get; set; }

	public InactiveModsConfig() : base("inactivemods.json")
	{
		Order = new() { IsModSettings = false, Name = "InactiveMods" };
	}
}
