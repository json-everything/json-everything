using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(AnyOfKeywordJsonConverter))]
	public class AnyOfKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "anyOf";

		public List<JsonSchema> Schemas { get; }

		public AnyOfKeyword(params JsonSchema[] values)
		{
			Schemas = values.ToList();
		}

		public AnyOfKeyword(IEnumerable<JsonSchema> values)
		{
			Schemas = values.ToList();
		}

		public ValidationResults Validate(ValidationContext context)
		{
			var subContexts = new List<ValidationContext>();
			var subResults = new List<ValidationResults>();
			var overallResult = false;
			for (var i = 0; i < Schemas.Count; i++)
			{
				var schema = Schemas[i];
				var subContext = ValidationContext.From(context, subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
				var results = schema.ValidateSubschema(subContext);
				overallResult |= results.IsValid;
				subResults.Add(results);
				subContexts.Add(subContext);
			}

			context.ConsolidateAnnotations(subContexts);
			var result = overallResult
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context);
			result.AddNestedResults(subResults);
			return result;
		}
	}

	public class AnyOfKeywordJsonConverter : JsonConverter<AnyOfKeyword>
	{
		public override AnyOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
				return new AnyOfKeyword(schemas);
			}
			
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
			return new AnyOfKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, AnyOfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(AnyOfKeyword.Name);
			writer.WriteStartArray();
			foreach (var schema in value.Schemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}