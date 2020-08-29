using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(EnumKeywordJsonConverter))]
	public class EnumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "enum";

		public IReadOnlyList<JsonElement> Values { get; }

		public EnumKeyword(params JsonElement[] values)
		{
			Values = values.Select(e => e.Clone()).ToList();
		}

		public EnumKeyword(IEnumerable<JsonElement> values)
		{
			Values = values.Select(e => e.Clone()).ToList();
		}

		public void Validate(ValidationContext context)
		{
			context.IsValid = Values.Contains(context.LocalInstance, JsonElementEqualityComparer.Instance);
			if (!context.IsValid)
				context.Message = "Expected value to match given value";
		}
	}

	internal class EnumKeywordJsonConverter : JsonConverter<EnumKeyword>
	{
		public override EnumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			if (document.RootElement.GetArrayLength() == 0)
				throw new JsonException("Enums must contain a value");

			return new EnumKeyword(document.RootElement.EnumerateArray());
		}
		public override void Write(Utf8JsonWriter writer, EnumKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ExamplesKeyword.Name);
			writer.WriteStartArray();
			foreach (var element in value.Values)
			{
				writer.WriteValue(element);
			}
			writer.WriteEndArray();
		}
	}
}