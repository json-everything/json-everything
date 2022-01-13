using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `deprecated`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[Vocabulary(Vocabularies.Metadata202012Id)]
	[JsonConverter(typeof(DeprecatedKeywordJsonConverter))]
	public class DeprecatedKeyword : IJsonSchemaKeyword, IEquatable<DeprecatedKeyword>
	{
		internal const string Name = "deprecated";

		/// <summary>
		/// Whether the schema is deprecated.
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Creates a new <see cref="DeprecatedKeyword"/>.
		/// </summary>
		/// <param name="value">Whether the schema is deprecated.</param>
		public DeprecatedKeyword(bool value)
		{
			Value = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			context.LocalResult.SetAnnotation(Name, Value);
			context.LocalResult.Pass();
			context.ExitKeyword(Name, true);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(DeprecatedKeyword? other)
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
			return Equals(obj as DeprecatedKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	internal class DeprecatedKeywordJsonConverter : JsonConverter<DeprecatedKeyword>
	{
		public override DeprecatedKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var value = reader.GetBoolean();

			return new DeprecatedKeyword(value);
		}
		public override void Write(Utf8JsonWriter writer, DeprecatedKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(DeprecatedKeyword.Name, value.Value);
		}
	}
}