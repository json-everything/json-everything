using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$comment`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(CommentKeywordJsonConverter))]
public class CommentKeyword : IJsonSchemaKeyword, IEquatable<CommentKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$comment";

	/// <summary>
	/// The comment value.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="CommentKeyword"/>.
	/// </summary>
	/// <param name="value">The comment value.</param>
	public CommentKeyword(string value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.Log(() => "$comment is to be ignored");
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation, EvaluationOptions options)
	{
		return Enumerable.Empty<Requirement>();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(CommentKeyword? other)
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
		return Equals(obj as CommentKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

internal class CommentKeywordJsonConverter : JsonConverter<CommentKeyword>
{
	public override CommentKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;

		return new CommentKeyword(str);
	}
	public override void Write(Utf8JsonWriter writer, CommentKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(CommentKeyword.Name, value.Value);
	}
}