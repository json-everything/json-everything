using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;

namespace TryJsonEverything.Models
{
	public class SchemaDraftConverter : JsonConverter<Draft>
	{
		public override Draft Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return reader.TokenType switch
			{
				JsonTokenType.String => FromString(reader.GetString()),
				JsonTokenType.Number => FromNumber(reader.GetInt32()),
				_ => throw new JsonException("Value must be a number or a string")
			};
		}

		private static Draft FromString(string? str)
		{
			return str switch
			{
				"6" => Draft.Draft6,
				"7" => Draft.Draft7,
				"2019-09" => Draft.Draft201909,
				"2020-12" => Draft.Draft202012,
				_ => throw new JsonException("String value must be '6', '7', '2019-09', or '2020-12'")
			};
		}

		private static Draft FromNumber(int num)
		{
			return num switch
			{
				6 => Draft.Draft6,
				7 => Draft.Draft7,
				_ => throw new JsonException("Numeric value must be 6 or 7")
			};
		}

		public override void Write(Utf8JsonWriter writer, Draft value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}