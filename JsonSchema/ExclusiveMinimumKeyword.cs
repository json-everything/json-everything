using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(ExclusiveMinimumKeywordJsonConverter))]
	public class ExclusiveMinimumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "exclusiveMinimum";

		public decimal Value { get; }

		public ExclusiveMinimumKeyword(decimal value)
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

	public class ExclusiveMinimumKeywordJsonConverter : JsonConverter<ExclusiveMinimumKeyword>
	{
		public override ExclusiveMinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new ExclusiveMinimumKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, ExclusiveMinimumKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(ExclusiveMinimumKeyword.Name, value.Value);
		}
	}
} 