using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `$anchor`.
/// </summary>
[SchemaKeyword(Name)]
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
	internal static readonly Regex AnchorPattern201909 = new("^[A-Za-z][-A-Za-z0-9.:_]*$");
	internal static readonly Regex AnchorPattern202012 = new("^[A-Za-z_][-A-Za-z0-9._]*$");

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
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		Span<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		if (context.EvaluatingAs <= SpecVersion.Draft201909 && !AnchorPattern201909.IsMatch(Anchor))
			throw new JsonSchemaException($"{Name} must conform to the regular expression '{AnchorPattern201909}'");

		if (context.EvaluatingAs >= SpecVersion.Draft202012 && !AnchorPattern202012.IsMatch(Anchor))
			throw new JsonSchemaException($"{Name} must conform to the regular expression '{AnchorPattern202012}'");

		return KeywordConstraint.Skip;
	}
}

/// <summary>
/// JSON converter for <see cref="AnchorKeyword"/>.
/// </summary>
public sealed class AnchorKeywordJsonConverter : WeaklyTypedJsonConverter<AnchorKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="AnchorKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override AnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString()!;
		if (!AnchorKeyword.AnchorPattern201909.IsMatch(uriString) && !AnchorKeyword.AnchorPattern202012.IsMatch(uriString))
			throw new JsonException("Expected anchor format");

		return new AnchorKeyword(uriString);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, AnchorKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Anchor);
	}
}