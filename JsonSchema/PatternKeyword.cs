using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(PatternKeywordJsonConverter))]
	public class PatternKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "pattern";

		public Regex Value { get; }

		public PatternKeyword(Regex value)
		{
			Value = value;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.String)
				return null;

			var str = context.Instance.GetString();
			return Value.IsMatch(str)
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, "The string value was not a match for the indicated regular expression");
		}
	}

	public class PatternKeywordJsonConverter : JsonConverter<PatternKeyword>
	{
		public override PatternKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			var regex = new Regex(str);

			return new PatternKeyword(regex);
		}
		public override void Write(Utf8JsonWriter writer, PatternKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(PatternKeyword.Name, value.Value.ToString());
		}
	}
}