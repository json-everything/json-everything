using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(ExclusiveMaximumKeywordJsonConverter))]
	public class ExclusiveMaximumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "exclusiveMaximum";
	
		public decimal Value { get; }

		public ExclusiveMaximumKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Number)
				return ValidationResults.Null;

			var number = context.Instance.GetDecimal();
			return Value > number
				? ValidationResults.Success()
				: ValidationResults.Fail($"{number} is not greater than {Value}");
		}
	}

	public class ExclusiveMaximumKeywordJsonConverter : JsonConverter<ExclusiveMaximumKeyword>
	{
		public override ExclusiveMaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new ExclusiveMaximumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, ExclusiveMaximumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(ExclusiveMaximumKeyword.Name, value.Value);
		}
	}
}