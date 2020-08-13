using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(OneOfKeywordJsonConverter))]
	public class OneOfKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "oneOf";

		public IReadOnlyList<JsonSchema> Schemas { get; }

		public OneOfKeyword(params JsonSchema[] values)
		{
			Schemas = values.ToList();
		}

		public OneOfKeyword(IEnumerable<JsonSchema> values)
		{
			Schemas = values.ToList();
		}

		public ValidationResults Validate(ValidationContext context)
		{
			var subContexts = new List<ValidationContext>();
			var subResults = new List<ValidationResults>();
			var validCount = 0;
			for (var i = 0; i < Schemas.Count; i++)
			{
				var schema = Schemas[i];
				var subContext = ValidationContext.From(context, subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
				var results = schema.ValidateSubschema(subContext);
				validCount += results.IsValid ? 1 : 0;
				subResults.Add(results);
				subContexts.Add(subContext);
			}

			context.ConsolidateAnnotations(subContexts);
			var result = validCount == 1
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"Expected 1 matching subschema but found {validCount}");
			result.AddNestedResults(subResults);
			return result;
		}
	}

	public class OneOfKeywordJsonConverter : JsonConverter<OneOfKeyword>
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