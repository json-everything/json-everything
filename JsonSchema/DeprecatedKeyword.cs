using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(DeprecatedKeywordJsonConverter))]
	public class DeprecatedKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "deprecated";

		public string Value { get; }

		public DeprecatedKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class DeprecatedKeywordJsonConverter : JsonConverter<DeprecatedKeyword>
	{
		public override DeprecatedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new DeprecatedKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, DeprecatedKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(DeprecatedKeyword.Name, value.Value);
		}
	}
}