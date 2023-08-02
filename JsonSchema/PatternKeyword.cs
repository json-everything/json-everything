using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// Handles `pattern`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[Vocabulary(Vocabularies.ValidationNextId)]
[JsonConverter(typeof(PatternKeywordJsonConverter))]
public class PatternKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "pattern";

	/// <summary>
	/// The regular expression.
	/// </summary>
	public Regex Value { get; }
	/// <summary>
	/// If the pattern is invalid or unsupported by <see cref="Regex"/>, it will appear here.
	/// </summary>
	/// <remarks>
	/// All validations will fail if this is populated.
	/// </remarks>
	public string? InvalidPattern { get; }

	/// <summary>
	/// Creates a new <see cref="PatternKeyword"/>.
	/// </summary>
	/// <param name="value">The regular expression.</param>
	public PatternKeyword(Regex value)
	{
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}

	private PatternKeyword(string invalidPattern)
	{
		InvalidPattern = invalidPattern;
		Value = new Regex($"^{Guid.NewGuid():N}$");
	}

	internal static PatternKeyword InvalidRegex(string pattern) => new(pattern);

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
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not SchemaValueType.String)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var str = evaluation.LocalInstance!.GetValue<string>();
		if (!Value.IsMatch(str))
			evaluation.Results.Fail(Name, ErrorMessages.Pattern, ("received", str), ("pattern", Value.ToString()));
	}
}

internal class PatternKeywordJsonConverter : JsonConverter<PatternKeyword>
{
	public override PatternKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException("Expected string");

		var str = reader.GetString()!;
		try
		{
			var regex = new Regex(str, RegexOptions.ECMAScript | RegexOptions.Compiled);

			return new PatternKeyword(regex);
		}
		catch
		{
			return PatternKeyword.InvalidRegex(str);
		}
	}
	public override void Write(Utf8JsonWriter writer, PatternKeyword value, JsonSerializerOptions options)
	{
		writer.WriteString(PatternKeyword.Name, value.Value.ToString());
	}
}

public static partial class ErrorMessages
{
	private static string? _invalidPattern;

	/// <summary>
	/// Gets or sets the error message for when the <see cref="PatternKeyword"/> contains
	/// an invalid or unsupported regular expression.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[pattern]] - the regular expression
	/// </remarks>
	public static string InvalidPattern
	{
		get => _invalidPattern ?? Get();
		set => _invalidPattern = value;
	}

	private static string? _pattern;

	/// <summary>
	/// Gets or sets the error message for <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[pattern]] - the number of subschemas that passed validation
	///
	/// The default messages are static and do not use these tokens as string values
	/// could be quite large.  They are provided to support custom messages.
	/// </remarks>
	public static string Pattern
	{
		get => _pattern ?? Get();
		set => _pattern = value;
	}
}