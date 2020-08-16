using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(ReadOnlyKeywordJsonConverter))]
	public class ReadOnlyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "readOnly";

		public string Value { get; }

		public ReadOnlyKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class ReadOnlyKeywordJsonConverter : JsonConverter<ReadOnlyKeyword>
	{
		public override ReadOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new ReadOnlyKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, ReadOnlyKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(ReadOnlyKeyword.Name, value.Value);
		}
	}
}