using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MinItemsKeywordJsonConverter))]
	public class MinItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minItems";

		public decimal Value { get; }

		public MinItemsKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Array)
				return null;

			var number = context.Instance.GetArrayLength();
			return Value <= number
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"Value has less than {Value} items");
		}
	}

	public class MinItemsKeywordJsonConverter : JsonConverter<MinItemsKeyword>
	{
		public override MinItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MinItemsKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinItemsKeyword.Name, value.Value);
		}
	}
}