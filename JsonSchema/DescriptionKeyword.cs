using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(DescriptionKeywordJsonConverter))]
	public class DescriptionKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "description";

		public string Value { get; }

		public DescriptionKeyword(string value)
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

	public class DescriptionKeywordJsonConverter : JsonConverter<DescriptionKeyword>
	{
		public override DescriptionKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new DescriptionKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, DescriptionKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(DescriptionKeyword.Name, value.Value);
		}
	}
}