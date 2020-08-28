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
	[JsonConverter(typeof(DefaultKeywordJsonConverter))]
	public class DefaultKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "default";

		public JsonElement Value { get; }

		public DefaultKeyword(JsonElement value)
		{
			Value = value.Clone();
		}

		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Value);
			context.IsValid = true;
		}
	}

	public class DefaultKeywordJsonConverter : JsonConverter<DefaultKeyword>
	{
		public override DefaultKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var element = JsonDocument.ParseValue(ref reader).RootElement;

			return new DefaultKeyword(element);
		}
		public override void Write(Utf8JsonWriter writer, DefaultKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DefaultKeyword.Name);
			value.Value.WriteTo(writer);
		}
	}
}