using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(PropertyNamesKeywordJsonConverter))]
	public class PropertyNamesKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "propertyNames";

		public JsonSchema Schema { get; }

		static PropertyNamesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public PropertyNamesKeyword(JsonSchema value)
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
			foreach (var name in context.LocalInstance.EnumerateObject().Select(p => p.Name))
			{
				var instance = name.AsJsonElement();
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{name}")),
					instance);
				Schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				context.NestedContexts.Add(subContext);
			}

			context.IsValid = overallResult;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allPropertyNames = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			// TODO: add message
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allPropertyNames);
			else if (allPropertyNames.Any())
				destContext.Annotations[Name] = allPropertyNames;
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

	public class PropertyNamesKeywordJsonConverter : JsonConverter<PropertyNamesKeyword>
	{
		public override PropertyNamesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new PropertyNamesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, PropertyNamesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(AdditionalPropertiesKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}