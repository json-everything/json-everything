using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Applicator201909Id)]
	[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
	public class DependentSchemasKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "dependentSchemas";

		public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

		public DependentSchemasKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Schemas = values;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			var evaluatedProperties = new List<string>();
			foreach (var property in Schemas)
			{
				var schema = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out _)) continue;
				
				var subContext = ValidationContext.From(context,
					subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
				schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				context.NestedContexts.Add(subContext);
				if (subContext.IsValid)
					evaluatedProperties.Add(name);
			}

			context.ConsolidateAnnotations();
			context.IsValid = overallResult;
			if (!context.IsValid)
				context.Message = $"The following properties failed their dependent schemas: {JsonSerializer.Serialize(evaluatedProperties)}";
		}

		public IRefResolvable ResolvePointerSegment(string value)
		{
			return Schemas.TryGetValue(value, out var schema) ? schema : null;
		}

		public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Schemas.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	public class DependentSchemasKeywordJsonConverter : JsonConverter<DependentSchemasKeyword>
	{
		public override DependentSchemasKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			return new DependentSchemasKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, DependentSchemasKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DependentSchemasKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Schemas)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}