using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `$recursiveAnchor`.
/// </summary>
[SchemaPriority(long.MinValue + 3)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[Vocabulary(Vocabularies.Core201909Id)]
[JsonConverter(typeof(RecursiveAnchorKeywordJsonConverter))]
public class RecursiveAnchorKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$recursiveAnchor";

	/// <summary>
	/// Gets the value.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="RecursiveAnchorKeyword"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public RecursiveAnchorKeyword(bool value)
	{
		Value = value;
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return KeywordConstraint.Skip;
	}
}

internal class RecursiveAnchorKeywordJsonConverter : JsonConverter<RecursiveAnchorKeyword>
{
	public override RecursiveAnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
			throw new JsonException("Expected boolean");

		var value = reader.GetBoolean();

		return new RecursiveAnchorKeyword(value);
	}
	public override void Write(Utf8JsonWriter writer, RecursiveAnchorKeyword value, JsonSerializerOptions options)
	{
		writer.WriteBoolean(RecursiveAnchorKeyword.Name, value.Value);
	}
}