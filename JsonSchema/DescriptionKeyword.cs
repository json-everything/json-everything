using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[JsonConverter(typeof(DescriptionKeywordJsonConverter))]
	public class DescriptionKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "description";

		public string Value { get; }

		public DescriptionKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Value);
			context.IsValid = true;
		}
	}

	internal class DescriptionKeywordJsonConverter : JsonConverter<DescriptionKeyword>
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