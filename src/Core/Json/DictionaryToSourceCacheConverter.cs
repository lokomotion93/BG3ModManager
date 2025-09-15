using DynamicData;

namespace ModManager.Json;

public interface IObjectWithId
{
	string Id { get; set; }
}

//Add the TValue type to _defaultSerializerSettings.Converters in JsonUtils
public class DictionaryToSourceCacheConverter<TValue> : JsonConverter<SourceCache<TValue, string>> where TValue : IObjectWithId
{
	private static readonly Type _type = typeof(SourceCache<TValue, string>);

	public override bool CanConvert(Type objectType) => _type.IsAssignableFrom(objectType);

	public override SourceCache<TValue, string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null) return default;

		//Since we're writing it as a dictionary, deserialize it to one first
		using var jsonDocument = JsonDocument.ParseValue(ref reader);
		var dict = JsonSerializer.Deserialize<Dictionary<string, TValue>>(jsonDocument, options);
		var cache = new SourceCache<TValue, string>(x => x.Id);
		if(dict != null)
		{
			foreach (var kvp in dict)
			{
				if (kvp.Value != null)
				{
					kvp.Value.Id = kvp.Key;
					cache.AddOrUpdate(kvp.Value);
				}
			}
		}
		return cache;
	}

	public override void Write(Utf8JsonWriter writer, SourceCache<TValue, string>? cache, JsonSerializerOptions options)
	{
		if (cache == null)
		{
			writer.WriteNullValue();
		}
		else
		{
			writer.WriteStartObject();
			foreach (var entry in cache.Items)
			{
				writer.WritePropertyName(entry.Id);
				JsonSerializer.Serialize(writer, entry, options);
			}
			writer.WriteEndObject();
		}
	}
}
