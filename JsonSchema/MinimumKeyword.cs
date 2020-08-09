using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword("minimum")]
	[JsonConverter(typeof(MinimumKeywordJsonConverter))]
	public class MinimumKeyword : IJsonSchemaKeyword
	{
		public decimal Value { get; }

		public MinimumKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Number)
				return ValidationResults.Null;

			var number = context.Instance.GetDecimal();
			return Value < number
				? ValidationResults.Success()
				: ValidationResults.Fail($"{number} is not less than {Value}");
		}
	}

	public class MinimumKeywordJsonConverter : JsonConverter<MinimumKeyword>
	{
		public override MinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MinimumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinimumKeyword value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}