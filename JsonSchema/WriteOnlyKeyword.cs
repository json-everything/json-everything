using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(WriteOnlyKeywordJsonConverter))]
	public class WriteOnlyKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "writeOnly";

		public string Value { get; }

		public WriteOnlyKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class WriteOnlyKeywordJsonConverter : JsonConverter<WriteOnlyKeyword>
	{
		public override WriteOnlyKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new WriteOnlyKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, WriteOnlyKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(WriteOnlyKeyword.Name, value.Value);
		}
	}
}