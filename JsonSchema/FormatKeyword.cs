using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(FormatKeywordJsonConverter))]
	public class FormatKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "format";

		public Format Value { get; }

		public FormatKeyword(Format value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.String)
			{
				context.IsValid = true;
				return;
			}

			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class FormatKeywordJsonConverter : JsonConverter<FormatKeyword>
	{
		public override FormatKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			var format = Formats.Get(str);

			return new FormatKeyword(format);
		}
		public override void Write(Utf8JsonWriter writer, FormatKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(FormatKeyword.Name, value.Value.Key);
		}
	}
}