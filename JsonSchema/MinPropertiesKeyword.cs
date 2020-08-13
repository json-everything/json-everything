using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(MinPropertiesKeywordJsonConverter))]
	public class MinPropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minProperties";

		public decimal Value { get; }

		public MinPropertiesKeyword(decimal value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Object)
				return null;

			var number = context.Instance.EnumerateObject().Count();
			return Value <= number
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"Value has more than {Value} properties");
		}
	}

	public class MinPropertiesKeywordJsonConverter : JsonConverter<MinPropertiesKeyword>
	{
		public override MinPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
				throw new JsonException("Expected number");

			var number = reader.GetDecimal();

			return new MinPropertiesKeyword(number);
		}
		public override void Write(Utf8JsonWriter writer, MinPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WriteNumber(MinPropertiesKeyword.Name, value.Value);
		}
	}
}  