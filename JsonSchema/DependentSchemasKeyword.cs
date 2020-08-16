using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
	public class DependentSchemasKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "dependentSchemas";

		public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

		static DependentSchemasKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public DependentSchemasKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Schemas = values;
		}

		public void Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Object)
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
				if (!context.Instance.TryGetProperty(name, out _)) continue;
				
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

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allDependentSchemas = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allDependentSchemas);
			else
				destContext.Annotations[Name] = allDependentSchemas;
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