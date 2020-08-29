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
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(AnchorKeywordJsonConverter))]
	public class AnchorKeyword : IJsonSchemaKeyword
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
			Anchor = anchor;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.IsValid = true;
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