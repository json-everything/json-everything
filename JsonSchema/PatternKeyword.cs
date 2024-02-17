using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.More;

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
public class PatternKeyword : IJsonSchemaKeyword, IKeywordHandler
{
	public static PatternKeyword Handler { get; } = new(string.Empty);

	bool IKeywordHandler.Evaluate(FunctionalEvaluationContext context)
	{
		if (!context.LocalSchema.AsObject().TryGetValue(Name, out var requirement, out _)) return true;

		string? pattern;
		if (requirement is not JsonValue reqValue || (pattern = reqValue.GetString()) is null)
			throw new Exception("maxLength must be a string");

		if (context.LocalInstance is not JsonValue value) return true;

		var str = value.GetString();
		if (str is null) return true;

		try
		{
			return Regex.IsMatch(str, pattern);
		}
		catch
		{
			return false;
		}
	}

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
			evaluation.Results.Fail(Name, ErrorMessages.GetPattern(context.Options.Culture)
				.ReplaceToken("received", str)
				.ReplaceToken("pattern", Value.ToString()));
	}
}

/// <summary>
/// JSON converter for <see cref="PatternKeyword"/>.
/// </summary>
public sealed class PatternKeywordJsonConverter : WeaklyTypedJsonConverter<PatternKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="PatternKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
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

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PatternKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Value.ToString());
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="PatternKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[pattern]] - the regular expression
	/// </remarks>
	public static string? Pattern { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="PatternKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[pattern]] - the regular expression
	/// </remarks>
	public static string GetPattern(CultureInfo? culture)
	{
		return Pattern ?? Get(culture);
	}
}