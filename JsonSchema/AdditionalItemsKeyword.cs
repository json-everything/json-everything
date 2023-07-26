using System;
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
[SchemaPriority(10)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[DependsOnAnnotationsFrom(typeof(ItemsKeyword))]
[JsonConverter(typeof(AdditionalItemsKeywordJsonConverter))]
public class AdditionalItemsKeyword : IJsonSchemaKeyword, ISchemaContainer, IEquatable<AdditionalItemsKeyword>
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
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);
		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.Array)
		{
			context.WrongValueKind(schemaValueType);
			return;
		}

		context.Options.LogIndentLevel++;
		var overallResult = true;
		if (!context.LocalResult.TryGetAnnotation(ItemsKeyword.Name, out var annotation))
		{
			context.NotApplicable(() => $"No annotations from {ItemsKeyword.Name}.");
			return;
		}
		context.Log(() => $"Annotation from {ItemsKeyword.Name}: {annotation}.");
		if (annotation!.GetValue<object>() is bool)
		{
			context.ExitKeyword(Name, context.LocalResult.IsValid);
			return;
		}

		var startIndex = (int)annotation.AsValue().GetInteger()!;
		var array = (JsonArray)context.LocalInstance!;
		for (int i = startIndex; i < array.Count; i++)
		{
			var i1 = i;
			context.Log(() => $"Evaluating item at index {i1}.");
			var item = array[i];
			context.Push(context.InstanceLocation.Combine(i), item ?? JsonNull.SignalNode,
				context.EvaluationPath.Combine(Name), Schema);
			context.Evaluate();
			overallResult &= context.LocalResult.IsValid;
			context.Log(() => $"Item at index {i1} {context.LocalResult.IsValid.GetValidityString()}.");
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
		}
		context.Options.LogIndentLevel--;
		context.LocalResult.SetAnnotation(Name, true);

		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, ConstraintBuilderContext context)
	{
		var itemsConstraint = localConstraints.GetKeywordConstraint<ItemsKeyword>();

		var subschemaConstraint = Schema.GetConstraint(JsonPointer.Create(Name), JsonPointer.Empty, context);
		subschemaConstraint.InstanceLocationGenerator = evaluation =>
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
			ChildDependencies = new[] { subschemaConstraint }
		};
		if (itemsConstraint != null)
			constraint.SiblingDependencies = new[] { itemsConstraint };
		return constraint;
	}

	private static void Evaluator(KeywordEvaluation evaluation)
	{
		if (evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.SetAnnotation(Name, evaluation.ChildEvaluations.Select(x => (JsonNode)x.RelativeInstanceLocation.Segments[0].Value!).ToJsonArray());
		else
			evaluation.Results.Fail();
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(AdditionalItemsKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Schema, other.Schema);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as AdditionalItemsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
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