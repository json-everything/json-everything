using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `$id`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(long.MinValue + 1)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(IdKeywordJsonConverter))]
public class IdKeyword : IIdKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$id";

	/// <summary>
	/// Defines the URI ID.
	/// </summary>
	public Uri Id { get; }

	/// <summary>
	/// Creates a new <see cref="IdKeyword"/>.
	/// </summary>
	/// <param name="id">The ID.</param>
	public IdKeyword(Uri id)
	{
		Id = id ?? throw new ArgumentNullException(nameof(id));
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		ConstraintBuilderContext context)
	{
		return KeywordConstraint.Skip;
	}
}

internal class IdKeywordJsonConverter : JsonConverter<IdKeyword>
{
	public override IdKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString();
		if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonException("Expected URI");

		return new IdKeyword(uri);
	}

	public override void Write(Utf8JsonWriter writer, IdKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(IdKeyword.Name, value.Id.OriginalString);
	}
}