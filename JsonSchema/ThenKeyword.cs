using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(ThenKeywordJsonConverter))]
	public class ThenKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "then";

		public JsonSchema Schema { get; }

		public ThenKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			var annotation = context.TryGetAnnotation(IfKeyword.Name);
			if (annotation == null || !(bool) annotation)
			{
				context.IsValid = true;
				return;
			}

			var subContext = ValidationContext.From(context);
			Schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);

			context.ConsolidateAnnotations();
			context.IsValid = subContext.IsValid;
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

	internal class ThenKeywordJsonConverter : JsonConverter<ThenKeyword>
	{
		public override ThenKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new ThenKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ThenKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ThenKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}