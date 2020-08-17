using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaPriority(long.MinValue + 1)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(DefsKeywordJsonConverter))]
	public class DefsKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "$defs";

		public IReadOnlyDictionary<string, JsonSchema> Definitions { get; }

		public DefsKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Definitions = values;
		}

		public void Validate(ValidationContext context)
		{
			context.IsValid = true;
		}

		public IRefResolvable ResolvePointerSegment(string value)
		{
			return Definitions.TryGetValue(value, out var schema) ? schema : null;
		}

		public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Definitions.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	public class DefsKeywordJsonConverter : JsonConverter<DefsKeyword>
	{
		public override DefsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			return new DefsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, DefsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DefsKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Definitions)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}