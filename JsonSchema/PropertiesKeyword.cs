using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `properties`.
	/// </summary>
	[Applicator]
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
	public class PropertiesKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "properties";

		/// <summary>
		/// The property schemas.
		/// </summary>
		public IReadOnlyDictionary<string, JsonSchema> Properties { get; }

		static PropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		/// <summary>
		/// Creates a new <see cref="PropertiesKeyword"/>.
		/// </summary>
		/// <param name="values">The property schemas.</param>
		public PropertiesKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Properties = values;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			var evaluatedProperties = new List<string>();
			foreach (var property in Properties)
			{
				var schema = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out var item)) continue;
				
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{name}")),
					item,
					context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
				schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				if (!overallResult && context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
				if (subContext.IsValid)
					evaluatedProperties.Add(name);
			}

			if (overallResult)
			{
				if (context.TryGetAnnotation(Name) is List<string> annotation)
					annotation.AddRange(evaluatedProperties);
				else
					context.SetAnnotation(Name, evaluatedProperties);
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
				destContext.SetAnnotation(Name, allProperties);
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			return Properties.TryGetValue(value, out var schema) ? schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Properties.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	internal class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
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
			writer.WriteStartObject();
			foreach (var kvp in value.Properties)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}