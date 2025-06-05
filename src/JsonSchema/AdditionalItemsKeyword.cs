﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `additionalItems`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[DependsOnAnnotationsFrom(typeof(ItemsKeyword))]
[JsonConverter(typeof(AdditionalItemsKeywordJsonConverter))]
public class AdditionalItemsKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "additionalItems";

	/// <summary>
	/// The schema by which to evaluate additional items.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="AdditionalItemsKeyword"/>.
	/// </summary>
	/// <param name="value">The keyword's schema.</param>
	public AdditionalItemsKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, ReadOnlySpan<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var itemsConstraint = localConstraints.GetKeywordConstraint<ItemsKeyword>();
		if (itemsConstraint == null) return KeywordConstraint.Skip;

		var subschemaConstraint = Schema.GetConstraint(JsonPointer_Old.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer_Old.Empty, context);
		subschemaConstraint.InstanceLocator = LocateInstances;

		var constraint = new KeywordConstraint(Name, Evaluator)
		{
			SiblingDependencies = [itemsConstraint],
			ChildDependencies = [subschemaConstraint]
		};
		return constraint;
	}

	private static IEnumerable<JsonPointer_Old> LocateInstances(KeywordEvaluation evaluation)
	{
		if (evaluation.LocalInstance is not JsonArray array) yield break;

		var startIndex = 0;

		var itemsEvaluation = evaluation.GetKeywordEvaluation<ItemsKeyword>();
		if (itemsEvaluation != null) startIndex = itemsEvaluation.ChildEvaluations.Length;

		if (array.Count <= startIndex) yield break;

		for (int i = startIndex; i < array.Count; i++)
		{
			yield return CommonJsonPointers.GetNumberSegment(i);
		}
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		evaluation.Results.SetAnnotation(Name, true);

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="AdditionalItemsKeyword"/>.
/// </summary>
public sealed class AdditionalItemsKeywordJsonConverter : WeaklyTypedJsonConverter<AdditionalItemsKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="AdditionalItemsKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override AdditionalItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;

		return new AdditionalItemsKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, AdditionalItemsKeyword value, JsonSerializerOptions options)
	{
		options.Write(writer, value.Schema, JsonSchemaSerializerContext.Default.JsonSchema);
	}
}