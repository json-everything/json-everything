using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `$schema`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(long.MinValue)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(SchemaKeywordJsonConverter))]
public class SchemaKeyword : IJsonSchemaKeyword, IEquatable<SchemaKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$schema";

	/// <summary>
	/// The meta-schema ID.
	/// </summary>
	public Uri Schema { get; }

	/// <summary>
	/// Creates a new <see cref="SchemaKeyword"/>.
	/// </summary>
	/// <param name="schema">The meta-schema ID.</param>
	public SchemaKeyword(Uri schema)
	{
		Schema = schema ?? throw new ArgumentNullException(nameof(schema));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var metaSchema = context.Options.SchemaRegistry.Get(Schema) as JsonSchema;
		if (metaSchema == null)
			throw new JsonSchemaException($"Cannot resolve meta-schema `{Schema}`");

		if (metaSchema.TryGetKeyword<VocabularyKeyword>(VocabularyKeyword.Name, out var vocabularyKeyword))
			context.UpdateMetaSchemaVocabs(vocabularyKeyword!.Vocabulary);

		if (!context.Options.ValidateAgainstMetaSchema)
		{
			context.ExitKeyword(Name, true);
			return;
		}

		context.Log(() => "Validating against meta-schema.");
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(context.LocalSchema));
		var schemaAsJson = document.RootElement;
		var newOptions = EvaluationOptions.From(context.Options);
		newOptions.ValidateAgainstMetaSchema = false;
		var results = metaSchema.Evaluate(schemaAsJson, newOptions);

		if (!results.IsValid)
			context.LocalResult.Fail(Name, ErrorMessages.MetaSchemaValidation, ("uri", Schema.OriginalString));
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(SchemaKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Schema, other.Schema);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as SchemaKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
	}
}

internal class SchemaKeywordJsonConverter : JsonConverter<SchemaKeyword>
{
	public override SchemaKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString();
		if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
			throw new JsonException("Expected absolute URI");

		return new SchemaKeyword(uri);
	}

	public override void Write(Utf8JsonWriter writer, SchemaKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(SchemaKeyword.Name, value.Schema.OriginalString);
	}
}

public static partial class ErrorMessages
{
	private static string? _metaSchemaValidation;

	/// <summary>
	/// Gets or sets the error message for when the schema cannot be validated
	/// against the meta-schema.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the URI of the meta-schema
	/// </remarks>
	public static string MetaSchemaValidation
	{
		get => _metaSchemaValidation ?? Get();
		set => _metaSchemaValidation = value;
	}
}