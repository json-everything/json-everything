using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MultipleOfKeywordJsonConverter))]
	public class MultipleOfKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "multipleOf";

		public decimal Value { get; }

		public MultipleOfKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Number)
				return null;

			var number = context.Instance.GetDecimal();
			return number % Value == 0
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"{number} a multiple of {Value}");
		}
	}

	public class MultipleOfKeywordJsonConverter : JsonConverter<MultipleOfKeyword>
	{
		public override MultipleOfKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MultipleOfKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MultipleOfKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MultipleOfKeyword.Name, value.Value);
		}
	}
}