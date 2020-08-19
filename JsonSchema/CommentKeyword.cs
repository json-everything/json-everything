using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(CommentKeywordJsonConverter))]
	public class CommentKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$comment";

		public string Value { get; }

		public CommentKeyword(string value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class CommentKeywordJsonConverter : JsonConverter<CommentKeyword>
	{
		public override CommentKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new CommentKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, CommentKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(CommentKeyword.Name, value.Value);
		}
	}
}