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
	[JsonConverter(typeof(AdditionalPropertiesKeywordJsonConverter))]
	public class AdditionalPropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "additionalProperties";

		public JsonSchema Schema { get; }

		static AdditionalPropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		public AdditionalPropertiesKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Object)
				return null;

			var subResults = new List<ValidationResults>();
			var overallResult = true;
			var annotation = context.TryGetAnnotation(PropertiesKeyword.Name);
			var evaluatedProperties = annotation as List<string> ?? new List<string>();
			annotation = context.TryGetAnnotation(PatternPropertiesKeyword.Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			var additionalProperties = context.Instance.EnumerateObject().Where(p => !evaluatedProperties.Contains(p.Name)).ToList();
			foreach (var property in additionalProperties)
			{
				if (!context.Instance.TryGetProperty(property.Name, out var item)) continue;

				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{property.Name}")),
					item);
				var results = Schema.ValidateSubschema(subContext);
				overallResult &= results.IsValid;
				subResults.Add(results);
			}

			context.Annotations[Name] = true;
			var result = overallResult
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context);

			result.AddNestedResults(subResults);
			return result;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			if (sourceContexts.Select(c => c.TryGetAnnotation(Name)).OfType<bool>().Any())
				destContext.Annotations[Name] = true;
		}
	}

	public class AdditionalPropertiesKeywordJsonConverter : JsonConverter<AdditionalPropertiesKeyword>
	{
		public override AdditionalPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new AdditionalPropertiesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, AdditionalPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(AdditionalPropertiesKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}