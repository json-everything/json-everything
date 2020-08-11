using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MinimumKeywordJsonConverter))]
	public class MinimumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minimum";

		public decimal Value { get; }

		public MinimumKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Number)
				return null;

			var number = context.Instance.GetDecimal();
			return Value <= number
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"{number} is not less than or equal to {Value}");
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
			writer.WriteNumber(MinimumKeyword.Name, value.Value);
		}
	}
}