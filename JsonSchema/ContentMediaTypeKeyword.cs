using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `contentMediaType`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Content201909Id)]
[Vocabulary(Vocabularies.Content202012Id)]
[Vocabulary(Vocabularies.ContentNextId)]
[JsonConverter(typeof(ContentMediaTypeKeywordJsonConverter))]
public class ContentMediaTypeKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "contentMediaType";

	/// <summary>
	/// The media type.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="ContentMediaTypeKeyword"/>.
	/// </summary>
	/// <param name="value">The media type.</param>
	public ContentMediaTypeKeyword(string value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return KeywordConstraint.SimpleAnnotation(Name, Value);
	}
}

internal class ContentMediaTypeKeywordJsonConverter : JsonConverter<ContentMediaTypeKeyword>
{
	public override ContentMediaTypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;

		return new ContentMediaTypeKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, ContentMediaTypeKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(ContentMediaTypeKeyword.Name, value.Value);
	}
}