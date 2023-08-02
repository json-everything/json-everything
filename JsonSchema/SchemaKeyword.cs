using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$schema`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(SchemaKeywordJsonConverter))]
public class SchemaKeyword : IJsonSchemaKeyword
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
		if (!context.Options.ValidateAgainstMetaSchema)
			return KeywordConstraint.Skip;

		var metaSchema = context.Options.SchemaRegistry.Get(Schema) as JsonSchema;
		if (metaSchema == null)
			throw new JsonSchemaException($"Cannot resolve meta-schema `{Schema}`");

		context.Options.ValidateAgainstMetaSchema = false;
		var metaSchemaConstraint = metaSchema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation.Combine(Name), JsonPointer.Empty, context);
		context.Options.ValidateAgainstMetaSchema = true;

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = new[] { metaSchemaConstraint }
		};
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (!evaluation.ChildEvaluations[0].Results.IsValid)
			evaluation.Results.Fail(Name, ErrorMessages.MetaSchemaValidation, ("uri", Schema.OriginalString));
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