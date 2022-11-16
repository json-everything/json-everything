﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$dynamicAnchor`.
/// </summary>
[SchemaPriority(long.MinValue + 3)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(DynamicAnchorKeywordJsonConverter))]
public class DynamicAnchorKeyword : IJsonSchemaKeyword, IAnchorProvider, IEquatable<DynamicAnchorKeyword>
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
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		context.ExitKeyword(Name, true);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation)
	{
		throw new NotImplementedException();
	}

	void IAnchorProvider.RegisterAnchor(SchemaRegistry registry, Uri currentUri, JsonSchema schema)
	{
		registry.RegisterAnchor(currentUri, Value, schema);
		registry.RegisterDynamicAnchor(currentUri, Value, schema);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(DynamicAnchorKeyword? other)
	{
		return true;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as DynamicAnchorKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
		return base.GetHashCode();
	}
}

internal class DynamicAnchorKeywordJsonConverter : JsonConverter<DynamicAnchorKeyword>
{
	public override DynamicAnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString()!;
		if (!AnchorKeyword.AnchorPattern.IsMatch(uriString))
			throw new JsonException("Expected anchor format");

		return new DynamicAnchorKeyword(uriString);
	}
	public override void Write(Utf8JsonWriter writer, DynamicAnchorKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(DynamicAnchorKeyword.Name, value.Value);
	}
}