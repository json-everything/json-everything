using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
	public class PatternPropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "patternProperties";

		public IReadOnlyDictionary<Regex, JsonSchema> Patterns { get; }

		static PatternPropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values)
		{
			Patterns = values;
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
			var instanceProperties = context.LocalInstance.EnumerateObject().ToList();
			foreach (var entry in Patterns)
			{
				var schema = entry.Value;
				var pattern = entry.Key;
				foreach (var instanceProperty in instanceProperties.Where(p => pattern.IsMatch(p.Name)))
				{
					var subContext = ValidationContext.From(context,
						context.InstanceLocation.Combine(PointerSegment.Create($"{instanceProperty.Name}")),
						instanceProperty.Value,
						context.SchemaLocation.Combine(PointerSegment.Create($"{pattern}")));
					schema.ValidateSubschema(subContext);
					overallResult &= subContext.IsValid;
					context.NestedContexts.Add(subContext);
					evaluatedProperties.Add(instanceProperty.Name);
				}
			}

			if (overallResult)
			{
				if (context.TryGetAnnotation(Name) is List<string> annotation)
					annotation.AddRange(evaluatedProperties);
				else
					context.Annotations[Name] = evaluatedProperties;
			}
			// TODO: add message
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
				destContext.Annotations[Name] = allProperties;
		}

		public IRefResolvable ResolvePointerSegment(string value)
		{
			var regex = new Regex(value);
			return Patterns.TryGetValue(regex, out var schema) ? schema : null;
		}
	}

	public class PatternPropertiesKeywordJsonConverter : JsonConverter<PatternPropertiesKeyword>
	{
		public override PatternPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schemas = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)
				.ToDictionary(kvp => new Regex(kvp.Key), kvp => kvp.Value);
			return new PatternPropertiesKeyword(schemas);
		}
		public override void Write(Utf8JsonWriter writer, PatternPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(PatternPropertiesKeyword.Name);
			writer.WriteStartObject();
			foreach (var schema in value.Patterns)
			{
				writer.WritePropertyName(schema.Key.ToString());
				JsonSerializer.Serialize(writer, schema.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}