using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `oneOf`.
	/// </summary>
	[Applicator]
	[SchemaPriority(20)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(OneOfKeywordJsonConverter))]
	public class OneOfKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "oneOf";

		/// <summary>
		/// The keywords schema collection.
		/// </summary>
		public IReadOnlyList<JsonSchema> Schemas { get; }

		/// <summary>
		/// Creates a new <see cref="OneOfKeyword"/>.
		/// </summary>
		/// <param name="values">The keywords schema collection.</param>
		public OneOfKeyword(params JsonSchema[] values)
		{
			Schemas = values.ToList();
		}

		/// <summary>
		/// Creates a new <see cref="OneOfKeyword"/>.
		/// </summary>
		/// <param name="values">The keywords schema collection.</param>
		public OneOfKeyword(IEnumerable<JsonSchema> values)
		{
			Schemas = values.ToList();
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			var validCount = 0;
			for (var i = 0; i < Schemas.Count; i++)
			{
				var schema = Schemas[i];
				var subContext = ValidationContext.From(context, subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
				schema.ValidateSubschema(subContext);
				validCount += subContext.IsValid ? 1 : 0;
				if (validCount > 1 && context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
			}

			context.ConsolidateAnnotations();
			context.IsValid = validCount == 1;
			if (!context.IsValid)
				context.Message = $"Expected 1 matching subschema but found {validCount}";
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			if (!int.TryParse(value, out var index)) return null;
			if (index < 0 || Schemas.Count <= index) return null;

			return Schemas[index];
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Schemas)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	internal class OneOfKeywordJsonConverter : JsonConverter<OneOfKeyword>
	{
		public override OneOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
				return new OneOfKeyword(schemas);
			}
			
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
			return new OneOfKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, OneOfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(OneOfKeyword.Name);
			writer.WriteStartArray();
			foreach (var schema in value.Schemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}