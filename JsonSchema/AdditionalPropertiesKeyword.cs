using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[Applicator]
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(AdditionalPropertiesKeywordJsonConverter))]
	public class AdditionalPropertiesKeyword : IJsonSchemaKeyword, IRefResolvable
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

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			var annotation = context.TryGetAnnotation(PropertiesKeyword.Name);
			var evaluatedProperties = (annotation as List<string>)?.ToList() ?? new List<string>();
			annotation = context.TryGetAnnotation(PatternPropertiesKeyword.Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			var additionalProperties = context.LocalInstance.EnumerateObject().Where(p => !evaluatedProperties.Contains(p.Name)).ToList();
			evaluatedProperties.Clear();
			foreach (var property in additionalProperties)
			{
				if (!context.LocalInstance.TryGetProperty(property.Name, out var item)) continue;

				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{property.Name}")),
					item);
				Schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				if (subContext.IsValid)
					evaluatedProperties.Add(property.Name);
				else if (context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
			}

			if (overallResult)
			{
				if (context.TryGetAnnotation(Name) is List<string> list)
					list.AddRange(evaluatedProperties);
				else
					context.SetAnnotation(Name, evaluatedProperties);
			}
			context.IsValid = overallResult;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allProperties = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allProperties);
			else if (allProperties.Any())
				destContext.SetAnnotation(Name, allProperties);
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