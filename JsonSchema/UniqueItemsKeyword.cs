using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `uniqueItems`.
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
[JsonConverter(typeof(UniqueItemsKeywordJsonConverter))]
public class UniqueItemsKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "uniqueItems";

	/// <summary>
	/// Whether items should be unique.
	/// </summary>
	public bool Value { get; }

	/// <summary>
	/// Creates a new <see cref="UniqueItemsKeyword"/>.
	/// </summary>
	/// <param name="value">Whether items should be unique.</param>
	public UniqueItemsKeyword(bool value)
	{
		Value = value;
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
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (!Value || evaluation.LocalInstance is not JsonArray array)
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var duplicates = new List<(int, int)>();
		for (int i = 0; i < array.Count - 1; i++)
		for (int j = i + 1; j < array.Count; j++)
		{
			if (array[i].IsEquivalentTo(array[j]))
				duplicates.Add((i, j));
		}

		if (duplicates.Count != 0)
		{
			var pairs = string.Join(", ", duplicates.Select(d => $"({d.Item1}, {d.Item2})"));
			evaluation.Results.Fail(Name, ErrorMessages.GetUniqueItems(context.Options.Culture)
				.ReplaceToken("duplicates", pairs));
		}
	}
}

/// <summary>
/// JSON converter for <see cref="UniqueItemsKeyword"/>.
/// </summary>
public sealed class UniqueItemsKeywordJsonConverter : WeaklyTypedJsonConverter<UniqueItemsKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="UniqueItemsKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override UniqueItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
			throw new JsonException("Expected boolean");

		var number = reader.GetBoolean();

		return new UniqueItemsKeyword(number);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, UniqueItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WriteBooleanValue(value.Value);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="UniqueItemsKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[duplicates]] - the indices of duplicate pairs as a comma-delimited list of "(x, y)" items
	/// </remarks>
	public static string? UniqueItems { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="UniqueItemsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[duplicates]] - the indices of duplicate pairs as a comma-delimited list of "(x, y)" items
	/// </remarks>
	public static string GetUniqueItems(CultureInfo? culture)
	{
		return UniqueItems ?? Get(culture);
	}
}