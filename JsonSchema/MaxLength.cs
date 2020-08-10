using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MaxLengthKeywordJsonConverter))]
	public class MaxLengthKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maxLength";

		public decimal Value { get; }

		public MaxLengthKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.String)
				return ValidationResults.Null;

			var str = context.Instance.GetString();
			return Value >= str.Length
				? ValidationResults.Success()
				: ValidationResults.Fail($"Value is not shorter than or equal to {Value} characters");
		}
	}

	public class MaxLengthKeywordJsonConverter : JsonConverter<MaxLengthKeyword>
	{
		public override MaxLengthKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MaxLengthKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MaxLengthKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MaxLengthKeyword.Name, value.Value);
		}
	}
}