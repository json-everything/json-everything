using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Represents the `ordering` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.ArrayExtId)]
[JsonConverter(typeof(OrderingKeywordJsonConverter))]
public class OrderingKeyword : IJsonSchemaKeyword
{
	private enum Ordering
	{
		Before = -1,
		Same,
		After
	}

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "ordering";

	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IEnumerable<OrderingSpecifier> Specifiers { get; }

	/// <summary>
	/// Creates an instance of the <see cref="OrderingKeyword"/> class.
	/// </summary>
	/// <param name="specifiers">The collection of keywords and references.</param>
	public OrderingKeyword(IEnumerable<OrderingSpecifier> specifiers)
	{
		Specifiers = specifiers;
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
		if (evaluation.LocalInstance is not JsonArray array) return;
		if (array.Count <= 1) return;

		var i = 1;

		while (i < array.Count)
		{
			var basisItem = array[i-1];
			var currentItem = array[i];

			foreach (var specifier in Specifiers)
			{
				// TODO: successive specifiers only matter if the previous values are equal

				if (!specifier.By.TryEvaluate(basisItem, out var basisValue))
				{
					evaluation.Results.Fail(Name, "Item at index [[index]] does not have a value at [[pointerOld]]"
							.ReplaceToken("index", i-1)
							.ReplaceToken("pointerOld", specifier.By, JsonSchemaArrayExtSerializerContext.Default.JsonPointer)
						);
					return;
				}

				if (!specifier.By.TryEvaluate(currentItem, out var currentValue))
				{
					evaluation.Results.Fail(Name, "Item at index [[index]] does not have a value at [[pointerOld]]"
							.ReplaceToken("index", i)
							.ReplaceToken("pointerOld", specifier.By, JsonSchemaArrayExtSerializerContext.Default.JsonPointer)
						);
					return;
				}

				var check = GetOrdering(basisValue, currentValue, specifier);

				if (check.Ordering == Ordering.Before) break;
				if (check.Ordering == Ordering.After)
				{
					// basis should be after current
					evaluation.Results.Fail(Name, "Item at index [[index]] is not in order"
						.ReplaceToken("index", i));
					return;
				}
			}

			i++;
		}
	}

	private static (Ordering? Ordering, string? ErrorMessage) GetOrdering(JsonNode? basis, JsonNode? current, OrderingSpecifier specifier)
	{
		if (basis is not JsonValue vBasis || current is not JsonValue vCurrent)
			return (null, "Comparisons may only occur between strings or numbers");

		if (vBasis.TryGetValue(out string? sBasis))
		{
			var compareOptions = specifier.IgnoreCase ? CompareOptions.OrdinalIgnoreCase : CompareOptions.Ordinal;
			if (vCurrent.TryGetValue(out string? sCurrent))
			{
				var comparison = string.Compare(sBasis, sCurrent, specifier.Culture, compareOptions);
				return (AdjustForDirection(ComparisonToOrdering(comparison), specifier.Direction), null);
			}

			return (null, "Comparisons must occur between values of the same type");
		}

		var nBasis = vBasis.GetNumber();
		var nCurrent = vCurrent.GetNumber();
		if (!nBasis.HasValue || !nCurrent.HasValue)
			return (null, "Comparisons must occur between values of the same type");

		return (AdjustForDirection(ComparisonToOrdering(nBasis.Value.CompareTo(nCurrent)), specifier.Direction), null);
	}

	private static Ordering ComparisonToOrdering(int comparison) =>
		comparison switch
		{
			< 0 => Ordering.Before,
			0 => Ordering.Same,
			> 0 => Ordering.After
		};

	private static Ordering AdjustForDirection(Ordering ordering, Direction direction) =>
		direction == Direction.Descending
			? (Ordering)((int)ordering * -1)
			: ordering;
}

/// <summary>
/// JSON converter for <see cref="OrderingKeyword"/>.
/// </summary>
public sealed class OrderingKeywordJsonConverter : WeaklyTypedJsonConverter<OrderingKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="OrderingKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override OrderingKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("Expected array");

		var references = options.ReadList(ref reader, JsonSchemaArrayExtSerializerContext.Default.OrderingSpecifier)!;
		return new OrderingKeyword(references);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, OrderingKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(OrderingKeyword.Name);
		options.WriteList(writer, value.Specifiers, JsonSchemaArrayExtSerializerContext.Default.OrderingSpecifier);
	}
}