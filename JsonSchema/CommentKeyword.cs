using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$comment`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(CommentKeywordJsonConverter))]
	public class CommentKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$comment";

		/// <summary>
		/// The comment value.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Creates a new <see cref="CommentKeyword"/>.
		/// </summary>
		/// <param name="value">The comment value.</param>
		public CommentKeyword(string value)
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

	internal class CommentKeywordJsonConverter : JsonConverter<CommentKeyword>
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