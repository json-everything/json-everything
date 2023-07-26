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
public class RecursiveAnchorKeyword : IJsonSchemaKeyword, IEquatable<RecursiveAnchorKeyword>
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

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		context.Log(() => "Nothing to do");
		context.ExitKeyword(Name, true);
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return KeywordConstraint.Skip;
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(RecursiveAnchorKeyword? other)
	{
		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as RecursiveAnchorKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
		return base.GetHashCode();
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