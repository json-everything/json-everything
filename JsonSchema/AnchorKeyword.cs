using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$anchor`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue + 2)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[Vocabulary(Vocabularies.Core202012Id)]
	[JsonConverter(typeof(AnchorKeywordJsonConverter))]
	public class AnchorKeyword : IJsonSchemaKeyword, IAnchorProvider, IEquatable<AnchorKeyword>
	{
		internal const string Name = "$anchor";
		internal static readonly Regex AnchorPattern = new Regex("^[A-Za-z][-A-Za-z0-9.:_]*$");

		/// <summary>
		/// The value of the anchor.
		/// </summary>
		public string Anchor { get; }

		/// <summary>
		/// Creates a new <see cref="AnchorKeyword"/>.
		/// </summary>
		/// <param name="anchor">The anchor value.</param>
		public AnchorKeyword(string anchor)
		{
			Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			context.LocalResult.Pass();
			context.ExitKeyword(Name, true);
		}

		void IAnchorProvider.RegisterAnchor(SchemaRegistry registry, Uri currentUri, JsonSchema schema)
		{
			registry.RegisterAnchor(currentUri, Anchor, schema);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(AnchorKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Anchor == other.Anchor;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as AnchorKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Anchor.GetHashCode();
		}
	}

	internal class AnchorKeywordJsonConverter : JsonConverter<AnchorKeyword>
	{
		public override AnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!AnchorKeyword.AnchorPattern.IsMatch(uriString))
				throw new JsonException("Expected anchor format");

			return new AnchorKeyword(uriString);
		}

		public override void Write(Utf8JsonWriter writer, AnchorKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(AnchorKeyword.Name, value.Anchor);
		}
	}
}