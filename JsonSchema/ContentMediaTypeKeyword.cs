using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Content201909Id)]
	[JsonConverter(typeof(ContentMediaTypeKeywordJsonConverter))]
	public class ContentMediaTypeKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "contentMediaType";

		public string Value { get; }

		public ContentMediaTypeKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Value);
			context.IsValid = true;
		}
	}

	internal class ContentMediaTypeKeywordJsonConverter : JsonConverter<ContentMediaTypeKeyword>
	{
		public override ContentMediaTypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new ContentMediaTypeKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, ContentMediaTypeKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(ContentMediaTypeKeyword.Name, value.Value);
		}
	}
}