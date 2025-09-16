using ModManager.Models;

namespace ModManager.Json;
public class LarianVersionToStringConverter : JsonConverter<LarianVersion>
{
	private static readonly Type _type = typeof(LarianVersion);

	public override bool CanConvert(Type objectType) => _type.IsAssignableFrom(objectType);

	//public override bool CanConvert(Type objectType) => objectType == typeof(string) || objectType == typeof(int) || objectType == typeof(ulong);

	public override LarianVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null) return default;

		//Since we're writing it as a dictionary, deserialize it to one first
		using var jsonDocument = JsonDocument.ParseValue(ref reader);
		var text = jsonDocument.RootElement.GetRawText();
		var version = new LarianVersion(text);
		return version;
	}

	public override void Write(Utf8JsonWriter writer, LarianVersion version, JsonSerializerOptions options)
	{
		if (version == null)
		{
			writer.WriteNullValue();
		}
		else
		{
			writer.WriteStringValue(version.Version);
		}
	}
}
