using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MaxItemsKeywordJsonConverter))]
	public class MaxItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maxItems";

		public decimal Value { get; }

		public MaxItemsKeyword(decimal value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			var number = context.Instance.GetArrayLength();
			context.IsValid = Value >= number;
			if (!context.IsValid)
				context.Message = $"Value has more than {Value} items";
		}
	}

	public class MaxItemsKeywordJsonConverter : JsonConverter<MaxItemsKeyword>
	{
		public override MaxItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MaxItemsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaxItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MaxItemsKeyword.Name, value.Value);
		}
	}
}