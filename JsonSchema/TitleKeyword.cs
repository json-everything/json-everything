using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `title`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Metadata201909Id)]
[Vocabulary(Vocabularies.Metadata202012Id)]
[Vocabulary(Vocabularies.MetadataNextId)]
[JsonConverter(typeof(TitleKeywordJsonConverter))]
public class TitleKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "title";

	/// <summary>
	/// The title.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="TitleKeyword"/>.
	/// </summary>
	/// <param name="value">The title.</param>
	public TitleKeyword(string value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, ConstraintBuilderContext context)
	{
		evaluation.Results.SetAnnotation(Name, Value);
	}
}

internal class TitleKeywordJsonConverter : JsonConverter<TitleKeyword>
{
	public override TitleKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;

		return new TitleKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, TitleKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(TitleKeyword.Name, value.Value);
	}
}