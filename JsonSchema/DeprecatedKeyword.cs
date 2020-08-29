using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[SchemaDraft(Draft.Draft201909)]
	[JsonConverter(typeof(DeprecatedKeywordJsonConverter))]
	public class DeprecatedKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "deprecated";

		public bool Value { get; }

		public DeprecatedKeyword(bool value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Value);
			context.IsValid = true;
		}
	}

	internal class DeprecatedKeywordJsonConverter : JsonConverter<DeprecatedKeyword>
	{
		public override DeprecatedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True || reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var value = reader.GetBoolean();

			return new DeprecatedKeyword(value);
		}
		public override void Write(Utf8JsonWriter writer, DeprecatedKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(DeprecatedKeyword.Name, value.Value);
		}
	}
}