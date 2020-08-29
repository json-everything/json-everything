using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `contentMediaEncoding`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Content201909Id)]
	[JsonConverter(typeof(ContentMediaEncodingKeywordJsonConverter))]
	public class ContentMediaEncodingKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "contentMediaEncoding";

		/// <summary>
		/// The encoding value.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Creates a new <see cref="ContentMediaEncodingKeyword"/>.
		/// </summary>
		/// <param name="value">The encoding value.</param>
		public ContentMediaEncodingKeyword(string value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.SetAnnotation(Name, Value);
			context.IsValid = true;
		}
	}

	internal class ContentMediaEncodingKeywordJsonConverter : JsonConverter<ContentMediaEncodingKeyword>
	{
		public override ContentMediaEncodingKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new ContentMediaEncodingKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, ContentMediaEncodingKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(ContentMediaEncodingKeyword.Name, value.Value);
		}
	}
}