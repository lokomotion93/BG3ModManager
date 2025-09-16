using ModManager.Models.Mod;
using ModManager.Models.Mod.Order;

namespace ModManager.Json;

public class MigrationNamingPolicy : JsonNamingPolicy
{
	private readonly Dictionary<string, string> NameMapping = new Dictionary<string, string>()
	{
		["UUID"] = nameof(ModOrderMod.Id),
	};

	public override string ConvertName(string name)
	{
		return NameMapping.GetValueOrDefault(name, name);
	}
}