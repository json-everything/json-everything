using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Applicator201909Id)]
	[JsonConverter(typeof(ContainsKeywordJsonConverter))]
	public class ContainsKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "contains";

		public JsonSchema Schema { get; }

		public ContainsKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			var count = context.LocalInstance.GetArrayLength();
			for (int i = 0; i < count; i++)
			{
				// TODO: shortcut if flag output
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
					context.LocalInstance[i]);
				Schema.ValidateSubschema(subContext);
				context.NestedContexts.Add(subContext);
			}

			var found = context.NestedContexts.Count(r => r.IsValid);
			context.IsValid = found != 0;
			if (context.IsValid)
				context.SetAnnotation(Name, found);
			else
				context.Message = "Expected array to contain at least one item that matched the schema, but it did not";
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

	public class ContainsKeywordJsonConverter : JsonConverter<ContainsKeyword>
	{
		public override ContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new ContainsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ContainsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ContainsKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}