using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var itemsConstraint = localConstraints.GetKeywordConstraint<ItemsKeyword>();
		if (itemsConstraint == null) return KeywordConstraint.Skip;

		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer.Empty, context);
		subschemaConstraint.InstanceLocator = evaluation =>
		{
			if (evaluation.LocalInstance is not JsonArray array) return Array.Empty<JsonPointer>();

			var startIndex = 0;

			var itemsEvaluation = evaluation.GetKeywordEvaluation<ItemsKeyword>();
			if (itemsEvaluation != null)
				startIndex = itemsEvaluation.ChildEvaluations.Length;

			if (array.Count <= startIndex) return Array.Empty<JsonPointer>();

			return Enumerable.Range(startIndex, array.Count - startIndex).Select(x => JsonPointer.Create(x));
		};

		var constraint = new KeywordConstraint(Name, Evaluator)
		{
			SiblingDependencies = new[] { itemsConstraint },
			ChildDependencies = new[] { subschemaConstraint }
		};
		return constraint;
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		evaluation.Results.SetAnnotation(Name, true);

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

internal class AdditionalItemsKeywordJsonConverter : JsonConverter<AdditionalItemsKeyword>
{
	public override AdditionalItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new AdditionalItemsKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, AdditionalItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(AdditionalItemsKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}