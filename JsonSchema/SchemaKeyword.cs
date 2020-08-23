using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(SchemaKeywordJsonConverter))]
	public class SchemaKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$schema";

		public Uri Schema { get; }

		public SchemaKeyword(Uri schema)
		{
			Schema = schema;
		}

		public void Validate(ValidationContext context)
		{
			context.IsValid = true;
		}
	}

	public class SchemaKeywordJsonConverter : JsonConverter<SchemaKeyword>
	{
		public override SchemaKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
				throw new JsonException("Expected absolute URI");

			return new SchemaKeyword(uri);
		}

		public override void Write(Utf8JsonWriter writer, SchemaKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(SchemaKeyword.Name, value.Schema.OriginalString);
		}
	}
}