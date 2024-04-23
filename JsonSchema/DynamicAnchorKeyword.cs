using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `$dynamicAnchor`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(DynamicAnchorKeywordJsonConverter))]
public class DynamicAnchorKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$dynamicAnchor";

	/// <summary>
	/// Gets the anchor value.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="DynamicAnchorKeyword"/>.
	/// </summary>
	/// <param name="value">The anchor value.</param>
	public DynamicAnchorKeyword(string value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
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
		return KeywordConstraint.Skip;
	}
}

/// <summary>
/// JSON converter for <see cref="DynamicAnchorKeyword"/>.
/// </summary>
public sealed class DynamicAnchorKeywordJsonConverter : WeaklyTypedJsonConverter<DynamicAnchorKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="DynamicAnchorKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override DynamicAnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString()!;
		if (!AnchorKeyword.AnchorPattern202012.IsMatch(uriString))
			throw new JsonException("Expected anchor format");

		return new DynamicAnchorKeyword(uriString);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DynamicAnchorKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Value);
	}
}