using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
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
public class PatternKeyword : IJsonSchemaKeyword
{
	private readonly RegexOrPattern _regexOrPattern;

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "pattern";

	/// <summary>
	/// The regular expression.
	/// </summary>
	public string Pattern => _regexOrPattern;

	/// <summary>
	/// Returns the Regex Value of the keyword.
	/// </summary>
	[Obsolete($"Please use the '{nameof(Pattern)}' property instead.")]
	public Regex Value => _regexOrPattern.ToRegex();

	/// <remarks>
	/// (obsolete) All validations will fail if this is populated.
	/// </remarks>
	[Obsolete("This property is not used and will be removed with the next major version.")]
	public string? InvalidPattern { get; }

	/// <summary>
	/// Creates a new <see cref="PatternKeyword"/> based on a regular expression instance.
	/// </summary>
	/// <param name="value"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public PatternKeyword(Regex value)
	{
		_regexOrPattern = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Creates a new <see cref="PatternKeyword"/> based on a regular expression pattern.
	/// </summary>
	/// <param name="pattern"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public PatternKeyword([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
	{
		_regexOrPattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		ReadOnlySpan<KeywordConstraint> localConstraints,
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
		
		if (!_regexOrPattern.IsMatch(str))
			evaluation.Results.Fail(Name, ErrorMessages.GetPattern(context.Options.Culture)
				.ReplaceToken("received", str)
				.ReplaceToken("pattern", _regexOrPattern.ToString()));
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
		return new PatternKeyword(str);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, PatternKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Pattern);
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