using ModManager.Models.Mod.Order;

namespace ModManager.Json;

public class IModOrderEntryConverter : JsonConverter<IModOrderEntry>
{
	private static readonly Type _modType = typeof(ModOrderMod);
	private static readonly Type _containerType = typeof(ModOrderContainer);

	public override IModOrderEntry? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null) return default;

		try
		{
			using var jsonDocument = JsonDocument.ParseValue(ref reader);
			var root = jsonDocument.RootElement;
			if (root.TryGetProperty("Type", out var typeProp))
			{
				var entryTypeStr = typeProp.GetString();
				if (entryTypeStr.IsValid() && Enum.TryParse<ModEntryType>(entryTypeStr, out var entryType))
				{
					if (entryType == ModEntryType.Mod)
					{
						return JsonSerializer.Deserialize(root.GetRawText(), _modType, options) as IModOrderEntry;
					}
					else if (entryType == ModEntryType.Container)
					{
						return JsonSerializer.Deserialize(root.GetRawText(), _containerType, options) as IModOrderEntry;
					}
				}
			}
			else
			{
				//Old load order syntax, default to mod type (MigrationNamingPolicy should rename UUID to Id)
				return JsonSerializer.Deserialize(root.GetRawText(), _modType, options) as IModOrderEntry;
			}
		}
		catch(Exception ex)
		{
			DivinityApp.Log($"Error parsing load order entry:\n{ex}");
		}
		return null;
	}

	public override void Write(Utf8JsonWriter writer, IModOrderEntry? entry, JsonSerializerOptions options)
	{
		if (entry == null)
		{
			writer.WriteNullValue();
		}
		else
		{
			if(entry.Type == ModEntryType.Mod && entry is ModOrderMod mod)
			{
				JsonSerializer.Serialize(writer, mod, _modType, options);
			}
			else if(entry.Type == ModEntryType.Container && entry is ModOrderContainer container)
			{
				JsonSerializer.Serialize(writer, container, _containerType, options);
			}
		}
	}
}
