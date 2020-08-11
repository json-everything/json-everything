using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(AdditionalItemsKeywordJsonConverter))]
	public class AdditionalItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "additionalItems";

		public JsonSchema Value { get; }

		public AdditionalItemsKeyword(JsonSchema value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Array)
				return null;

			var subResults = new List<ValidationResults>();
			var overallResult = true;
			int startIndex = 0;
			if (context.Annotations.TryGetValue(ItemsKeyword.EvaluatedCount, out var annotation))
				startIndex = (int) annotation;
			foreach (var item in context.Instance.EnumerateArray().Skip(startIndex))
			{
				var results = Value.Validate(item);
				overallResult &= results.IsValid;
				subResults.Add(results);
			}

			var result = overallResult
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context);

			result.AddNestedResults(subResults);
			return result;
		}
	}

	public class AdditionalItemsKeywordJsonConverter : JsonConverter<AdditionalItemsKeyword>
	{
		public override AdditionalItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new AdditionalItemsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, AdditionalItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(AdditionalItemsKeyword.Name);
			JsonSerializer.Serialize(writer, value.Value, options);
		}
	}
}