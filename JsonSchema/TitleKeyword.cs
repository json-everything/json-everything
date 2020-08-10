using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(TitleKeywordJsonConverter))]
	public class TitleKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "title";

		public string Value { get; }

		public TitleKeyword(string value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.String)
				return ValidationResults.Null;

			using var document = JsonDocument.Parse($"\"{Value}\"");

			return ValidationResults.Annotation(context, document.RootElement);
		}
	}

	public class TitleKeywordJsonConverter : JsonConverter<TitleKeyword>
	{
		public override TitleKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new TitleKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, TitleKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(TitleKeyword.Name, value.Value);
		}
	}
}