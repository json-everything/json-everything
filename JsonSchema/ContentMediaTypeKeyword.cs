﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `contentMediaType`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Content201909Id)]
[Vocabulary(Vocabularies.Content202012Id)]
[Vocabulary(Vocabularies.ContentNextId)]
[JsonConverter(typeof(ContentMediaTypeKeywordJsonConverter))]
public class ContentMediaTypeKeyword : IJsonSchemaKeyword, IEquatable<ContentMediaTypeKeyword>
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
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.String)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		context.LocalResult.SetAnnotation(Name, Value);
		context.ExitKeyword(Name, true);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, DynamicScope scope, JsonPointer instanceLocation)
	{
		yield return new Requirement(subschemaPath, instanceLocation,
			(_, _, _, _) => new KeywordResult(Name, subschemaPath, scope.LocalScope, instanceLocation)
			{
				Annotation = Value
			});
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ContentMediaTypeKeyword? other)
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
		return Equals(obj as ContentMediaTypeKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Value.GetHashCode();
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