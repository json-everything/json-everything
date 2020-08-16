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

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Number)
			{
				context.IsValid = true;
				return;
			}

			var number = context.LocalInstance.GetDecimal();
			context.IsValid = number % Value == 0;
			if (!context.IsValid)
				context.Message = $"{number} a multiple of {Value}";
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