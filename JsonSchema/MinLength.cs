using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MinLengthKeywordJsonConverter))]
	public class MinLengthKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minLength";

		public decimal Value { get; }

		public MinLengthKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.String)
				return null;

			var str = context.Instance.GetString();
			return Value <= str.Length
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"Value is not longer than or equal to {Value} characters");
		}
	}

	public class MinLengthKeywordJsonConverter : JsonConverter<MinLengthKeyword>
	{
		public override MinLengthKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MinLengthKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinLengthKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinLengthKeyword.Name, value.Value);
		}
	}
}