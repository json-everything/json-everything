using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Metadata201909Id)]
	[JsonConverter(typeof(WriteOnlyKeywordJsonConverter))]
	public class WriteOnlyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "writeOnly";

		public bool Value { get; }

		public WriteOnlyKeyword(bool value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class WriteOnlyKeywordJsonConverter : JsonConverter<WriteOnlyKeyword>
	{
		public override WriteOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True || reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var str = reader.GetBoolean();

			return new WriteOnlyKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, WriteOnlyKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(WriteOnlyKeyword.Name, value.Value);
		}
	}
}