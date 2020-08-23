using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)] [SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Content201909Id)]
	[JsonConverter(typeof(ContentMediaEncodingKeywordJsonConverter))]
	public class ContentMediaEncodingKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "contentMediaEncoding";

		public string Value { get; }

		public ContentMediaEncodingKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class ContentMediaEncodingKeywordJsonConverter : JsonConverter<ContentMediaEncodingKeyword>
	{
		public override ContentMediaEncodingKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new ContentMediaEncodingKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, ContentMediaEncodingKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(ContentMediaEncodingKeyword.Name, value.Value);
		}
	}
}