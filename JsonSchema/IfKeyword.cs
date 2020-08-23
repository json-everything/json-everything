using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Applicator201909Id)]
	[JsonConverter(typeof(IfKeywordJsonConverter))]
	public class IfKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "if";

		public JsonSchema Schema { get; }

		public IfKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			var subContext = ValidationContext.From(context);
			Schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);

			context.Annotations[Name] = subContext.IsValid;
			context.ConsolidateAnnotations();
			context.IsValid = true;
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

	public class IfKeywordJsonConverter : JsonConverter<IfKeyword>
	{
		public override IfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new IfKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, IfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(IfKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}