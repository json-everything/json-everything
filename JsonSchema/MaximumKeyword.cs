using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword("maximum")]
	[JsonConverter(typeof(MaximumKeywordJsonConverter))]
	public class MaximumKeyword : IJsonSchemaKeyword
	{
		public decimal Value { get; }

		public MaximumKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Number)
				return ValidationResults.Null;

			var number = context.Instance.GetDecimal();
			return Value >= number
				? ValidationResults.Success()
				: ValidationResults.Fail($"{number} is not less than {Value}");
		}
	}

	public class MaximumKeywordJsonConverter : JsonConverter<MaximumKeyword>
	{
		public override MaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MaximumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaximumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber("maximum", value.Value);
		}
	}
}