using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `dependentSchemas`.
	/// </summary>
	[Applicator]
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(DependentSchemasKeywordJsonConverter))]
	public class DependentSchemasKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "dependentSchemas";

		/// <summary>
		/// The collection of "schema"-type dependencies.
		/// </summary>
		public IReadOnlyDictionary<string, JsonSchema> Schemas { get; }

		/// <summary>
		/// Creates a new <see cref="DependentSchemasKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of "schema"-type dependencies.</param>
		public DependentSchemasKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Schemas = values;
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
			foreach (var property in Schemas)
			{
				var schema = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out _)) continue;
				
				var subContext = ValidationContext.From(context,
					subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
				schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				if (!overallResult && context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
				if (subContext.IsValid)
					evaluatedProperties.Add(name);
			}

			context.ConsolidateAnnotations();
			context.IsValid = overallResult;
			if (!context.IsValid)
				context.Message = $"The following properties failed their dependent schemas: {JsonSerializer.Serialize(evaluatedProperties)}";
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			return Schemas.TryGetValue(value, out var schema) ? schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Schemas.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	internal class DependentSchemasKeywordJsonConverter : JsonConverter<DependentSchemasKeyword>
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