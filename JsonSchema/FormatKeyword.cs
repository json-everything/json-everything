using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema;

/// <summary>
/// Handles `format`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Format201909Id)]
[Vocabulary(Vocabularies.FormatAnnotation202012Id)]
[Vocabulary(Vocabularies.FormatAssertion202012Id)]
[Vocabulary(Vocabularies.FormatAnnotationNextId)]
[Vocabulary(Vocabularies.FormatAssertionNextId)]
[JsonConverter(typeof(FormatKeywordJsonConverter))]
public class FormatKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "format";

	private static readonly Uri[] _formatAssertionIds =
	{
		new(Vocabularies.Format201909Id),
		new(Vocabularies.FormatAssertion202012Id)
	};

	/// <summary>
	/// The format.
	/// </summary>
	public Format Value { get; }

	/// <summary>
	/// Creates a new <see cref="FormatKeyword"/>.
	/// </summary>
	/// <param name="value">The format.</param>
	public FormatKeyword(Format value)
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
		if (Value is UnknownFormat && context.Options.OnlyKnownFormats)
			return new KeywordConstraint(Name, (e, _) => e.Results.Fail(Name, ErrorMessages.UnknownFormat, ("format", Value.Key)));

		var requireValidation = context.Options.RequireFormatValidation;

		if (!requireValidation)
		{
			var vocabs = context.SchemaVocabs[context.Scope.LocalScope];
			if (vocabs != null)
			{
				foreach (var formatAssertionId in _formatAssertionIds)
				{
					if (vocabs.All(v => v.Id != formatAssertionId)) continue;

					// See https://github.com/json-schema-org/json-schema-spec/pull/1027#discussion_r530068335
					// for why we don't take the vocab value.
					// tl;dr - This implementation understands the assertion vocab, so we apply it,
					// even when the meta-schema says we're not required to.
					requireValidation = true;
					break;
				}
			}
		}

		return requireValidation
			? KeywordConstraint.SetAnnotation(Name, Value.Key)
			: new KeywordConstraint(Name, AssertionEvaluator);
	}

	private void AssertionEvaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (Value.Validate(evaluation.LocalInstance, out var errorMessage)) return;

		if (Value is UnknownFormat)
			evaluation.Results.Fail(Name, errorMessage);
		else if (errorMessage == null)
			evaluation.Results.Fail(Name, ErrorMessages.Format, ("format", Value.Key));
		else
			evaluation.Results.Fail(Name, ErrorMessages.FormatWithDetail, ("format", Value.Key), ("detail", errorMessage));
	}
}

internal class FormatKeywordJsonConverter : JsonConverter<FormatKeyword>
{
	public override FormatKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;
		var format = Formats.Get(str);

		return new FormatKeyword(format);
	}
	public override void Write(Utf8JsonWriter writer, FormatKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(FormatKeyword.Name, value.Value.Key);
	}
}

public static partial class ErrorMessages
{
	private static string? _unknownFormat;

	/// <summary>
	/// Gets or sets the error message for an unknown format.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string UnknownFormat
	{
		get => _unknownFormat ?? Get();
		set => _unknownFormat = value;
	}

	private static string? _format;

	/// <summary>
	/// Gets or sets the error message for the <see cref="FormatKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	/// </remarks>
	public static string Format
	{
		get => _format ?? Get();
		set => _format = value;
	}

	private static string? _formatWithDetail;

	/// <summary>
	/// Gets or sets the error message for the <see cref="FormatKeyword"/> with
	/// additional information from the format validation.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[format]] - the format key
	///   - [[detail]] - the format key
	/// </remarks>
	public static string FormatWithDetail
	{
		get => _formatWithDetail ?? Get();
		set => _formatWithDetail = value;
	}
}