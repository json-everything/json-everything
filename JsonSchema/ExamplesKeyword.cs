using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(ExamplesKeywordJsonConverter))]
	public class ExamplesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "examples";

		public IReadOnlyList<JsonElement> Values { get; }

		public ExamplesKeyword(params JsonElement[] values)
		{
			Values = values.Select(e => e.Clone()).ToList();
		}

		public ExamplesKeyword(IEnumerable<JsonElement> values)
		{
			Values = values.Select(e => e.Clone()).ToList();
		}

		public ValidationResults Validate(ValidationContext context)
		{
			context.Annotations[Name] = Values;
			return ValidationResults.Success(context);
		}
	}

	public class ExamplesKeywordJsonConverter : JsonConverter<ExamplesKeyword>
	{
		public override ExamplesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			return new ExamplesKeyword(document.RootElement.EnumerateArray());
		}
		public override void Write(Utf8JsonWriter writer, ExamplesKeyword value, JsonSerializerOptions options)
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