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
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Content201909Id)]
	[Vocabulary(Vocabularies.Content202012Id)]
	[JsonConverter(typeof(ContentMediaEncodingKeywordJsonConverter))]
	public class ContentMediaEncodingKeyword : IJsonSchemaKeyword, IEquatable<ContentMediaEncodingKeyword>
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
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (context.LocalInstance.ValueKind != JsonValueKind.String)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			context.LocalResult.SetAnnotation(Name, Value);
			context.LocalResult.Pass();
			context.ExitKeyword(Name, true);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ContentMediaEncodingKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value == other.Value;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as ContentMediaEncodingKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
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