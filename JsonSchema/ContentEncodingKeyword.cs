using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `contentMediaEncoding`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Content201909Id)]
[Vocabulary(Vocabularies.Content202012Id)]
[Vocabulary(Vocabularies.ContentNextId)]
[JsonConverter(typeof(ContentEncodingKeywordJsonConverter))]
public class ContentEncodingKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "contentEncoding";

	/// <summary>
	/// The encoding value.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="ContentEncodingKeyword"/>.
	/// </summary>
	/// <param name="value">The encoding value.</param>
	public ContentEncodingKeyword(string value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return new KeywordConstraint(Name, (e, _) => e.Results.SetAnnotation(Name, Value));
	}
}

internal class ContentEncodingKeywordJsonConverter : JsonConverter<ContentEncodingKeyword>
{
	public override ContentEncodingKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;

		return new ContentEncodingKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, ContentEncodingKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(ContentEncodingKeyword.Name, value.Value);
	}
}