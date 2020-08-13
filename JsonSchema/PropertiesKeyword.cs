using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
	public class PropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "properties";

		public IReadOnlyDictionary<string, JsonSchema> Properties { get; }

		static PropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public PropertiesKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Properties = values;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Object)
				return null;
			
			var subResults = new List<ValidationResults>();
			var overallResult = true;
			var evaluatedProperties = new List<string>();
			foreach (var property in Properties)
			{
				var schema = property.Value;
				var name = property.Key;
				if (!context.Instance.TryGetProperty(name, out var item)) continue;
				
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{name}")),
					item,
					context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
				var results = schema.ValidateSubschema(subContext);
				overallResult &= results.IsValid;
				subResults.Add(results);
				evaluatedProperties.Add(name);
			}

			context.Annotations[Name] = evaluatedProperties;

			var result = overallResult
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context);
			result.AddNestedResults(subResults);
			return result;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			object value;
			var allAnnotations = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.ToList();
			if (allAnnotations.OfType<bool>().Any())
				value = true;
			else
				value = allAnnotations.OfType<int>().DefaultIfEmpty(-1).Max();
			if (!Equals(value, -1))
				destContext.Annotations[Name] = value;
		}
	}

	public class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
	{
		public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			return new PropertiesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(PropertiesKeyword.Name);
			writer.WriteStartArray();
			foreach (var schema in value.Properties)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}