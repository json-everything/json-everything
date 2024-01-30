using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
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

		if (context.Options.SchemaRegistry.Get(Schema) is not JsonSchema metaSchema)
			throw new JsonSchemaException($"Cannot resolve meta-schema `{Schema}`");

		context.Options.ValidateAgainstMetaSchema = false;
		var metaSchemaConstraint = metaSchema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation.Combine(Name), JsonPointer.Empty, context);
		context.Options.ValidateAgainstMetaSchema = true;

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = [metaSchemaConstraint]
		};
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (!evaluation.ChildEvaluations[0].Results.IsValid)
			evaluation.Results.Fail(Name, ErrorMessages.GetMetaSchemaValidation(context.Options.Culture), ("uri", Schema.OriginalString));
	}
}

/// <summary>
/// JSON converter for <see cref="SchemaKeyword"/>.
/// </summary>
public sealed class SchemaKeywordJsonConverter : AotCompatibleJsonConverter<SchemaKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="SchemaKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override SchemaKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var uriString = reader.GetString();
		if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
			throw new JsonException("Expected absolute URI");

		return new SchemaKeyword(uri);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, SchemaKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Schema.OriginalString);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for when the schema cannot be validated
	/// against the meta-schema.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the URI of the meta-schema
	/// </remarks>
	public static string? MetaSchemaValidation { get; set; }

	/// <summary>
	/// Gets or sets the error message for when the schema cannot be validated
	/// against the meta-schema.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the URI of the meta-schema
	/// </remarks>
	public static string GetMetaSchemaValidation(CultureInfo? culture)
	{
		return MetaSchemaValidation ?? Get(culture);
	}
}