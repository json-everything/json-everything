using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// Handles `$anchor`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(long.MinValue + 2)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(AnchorKeywordJsonConverter))]
public class AnchorKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$anchor";
	internal static readonly Regex AnchorPattern = new("^[A-Za-z][-A-Za-z0-9.:_]*$");

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

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return KeywordConstraint.Skip;
	}
}

internal class AnchorKeywordJsonConverter : JsonConverter<AnchorKeyword>
{
	public override AnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString()!;
		if (!AnchorKeyword.AnchorPattern.IsMatch(uriString))
			throw new JsonException("Expected anchor format");

		return new AnchorKeyword(uriString);
	}

	public override void Write(Utf8JsonWriter writer, AnchorKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(AnchorKeyword.Name, value.Anchor);
	}
}