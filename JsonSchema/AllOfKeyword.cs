using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(20)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(AllOfKeywordJsonConverter))]
	public class AllOfKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "allOf";

		public IReadOnlyList<JsonSchema> Schemas { get; }

		public AllOfKeyword(params JsonSchema[] values)
		{
			Schemas = values.ToList();
		}

		public AllOfKeyword(IEnumerable<JsonSchema> values)
		{
			Schemas = values.ToList();
		}

		public void Validate(ValidationContext context)
		{
			var overallResult = true;
			for (var i = 0; i < Schemas.Count; i++)
			{
				var schema = Schemas[i];
				var subContext = ValidationContext.From(context, subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
				schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				context.NestedContexts.Add(subContext);
			}

			context.ConsolidateAnnotations();
			context.IsValid = overallResult;
		}
	}

	public class AllOfKeywordJsonConverter : JsonConverter<AllOfKeyword>
	{
		public override AllOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
				return new AllOfKeyword(schemas);
			}
			
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
			return new AllOfKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, AllOfKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(AllOfKeyword.Name);
			writer.WriteStartArray();
			foreach (var schema in value.Schemas)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}