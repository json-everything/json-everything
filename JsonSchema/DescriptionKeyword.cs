using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `description`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Metadata201909Id)]
	[Vocabulary(Vocabularies.Metadata202012Id)]
	[JsonConverter(typeof(DescriptionKeywordJsonConverter))]
	public class DescriptionKeyword : IJsonSchemaKeyword, IEquatable<DescriptionKeyword>
	{
		internal const string Name = "description";

		/// <summary>
		/// The description.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Creates a new <see cref="DescriptionKeyword"/>.
		/// </summary>
		/// <param name="value">The description.</param>
		public DescriptionKeyword(string value)
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
			context.LocalResult.SetAnnotation(Name, Value);
			context.LocalResult.Pass();
			context.ExitKeyword(Name, true);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(DescriptionKeyword? other)
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
			return Equals(obj as DescriptionKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	internal class DescriptionKeywordJsonConverter : JsonConverter<DescriptionKeyword>
	{
		public override DescriptionKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return new DescriptionKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, DescriptionKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(DescriptionKeyword.Name, value.Value);
		}
	}
}