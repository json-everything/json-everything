using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(20)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Content201909Id)]
	[JsonConverter(typeof(ContentSchemaKeywordJsonConverter))]
	public class ContentSchemaKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "contentSchema";

		public JsonSchema Schema { get; }

		public ContentSchemaKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			var subContext = ValidationContext.From(context,
				subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create(Name)));
			Schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);
			context.IsValid = !subContext.IsValid;
			context.ConsolidateAnnotations();
		}

		public IRefResolvable ResolvePointerSegment(string value)
		{
			return value == null ? Schema : null;
		}

		public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}
	}

	public class ContentSchemaKeywordJsonConverter : JsonConverter<ContentSchemaKeyword>
	{
		public override ContentSchemaKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new ContentSchemaKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ContentSchemaKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ContentSchemaKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}