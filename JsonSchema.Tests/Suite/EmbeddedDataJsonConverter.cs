using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Tests.Suite
{
	public class EmbeddedDataJsonConverter : JsonConverter<JsonElement>
	{
		public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			return document.RootElement.Clone();
		}

		public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}