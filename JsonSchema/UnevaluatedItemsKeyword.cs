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
/// Handles `unevaluatedItems`.
/// </summary>
[Applicator]
[SchemaPriority(30)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(UnevaluatedItemsKeywordJsonConverter))]
public class UnevaluatedItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<UnevaluatedItemsKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "unevaluatedItems";

	/// <summary>
	/// The schema by which to evaluate unevaluated items.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="UnevaluatedItemsKeyword"/>.
	/// </summary>
	/// <param name="value">The schema by which to evaluate unevaluated items.</param>
	public UnevaluatedItemsKeyword(JsonSchema value)
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
		int startIndex = 0;
		var annotations = context.LocalResult.GetAllAnnotations(PrefixItemsKeyword.Name).ToList();
		if (annotations.Any())
		{
			// ReSharper disable once AccessToModifiedClosure
			context.Log(() => $"Annotations from {PrefixItemsKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}.");
			if (annotations.Any(x => x!.AsValue().TryGetValue(out bool _))) // is only ever true or a number
			{
				context.ExitKeyword(Name, true);
				return;
			}
			startIndex = annotations.Max(x => x!.AsValue().TryGetValue(out int i) ? i : 0);
		}
		else
			context.Log(() => $"No annotations from {PrefixItemsKeyword.Name}.");
		annotations = context.LocalResult.GetAllAnnotations(ItemsKeyword.Name).ToList();
		if (annotations.Any())
		{
			// ReSharper disable once AccessToModifiedClosure
			context.Log(() => $"Annotations from {ItemsKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}.");
			if (annotations.Any(x => x!.AsValue().TryGetValue(out bool _))) // is only ever true or a number
			{
				context.ExitKeyword(Name, true);
				return;
			}
			startIndex = annotations.Max(x => x!.AsValue().TryGetValue(out int i) ? i : 0);
		}
		else
			context.Log(() => $"No annotations from {ItemsKeyword.Name}.");
		annotations = context.LocalResult.GetAllAnnotations(AdditionalItemsKeyword.Name).ToList();
		if (annotations.Any()) // is only ever true
		{
			context.Log(() => $"Annotation from {AdditionalItemsKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}.");
			context.ExitKeyword(Name, true);
			return;
		}
		context.Log(() => $"No annotations from {AdditionalItemsKeyword.Name}.");
		annotations = context.LocalResult.GetAllAnnotations(Name).ToList();
		if (annotations.Any()) // is only ever true
		{
			context.Log(() => $"Annotation from {Name}: {annotations.ToJsonArray().AsJsonString()}.");
			context.ExitKeyword(Name, true);
			return;
		}
		context.Log(() => $"No annotations from {Name}.");
		var array = (JsonArray)context.LocalInstance!;
		var indicesToEvaluate = Enumerable.Range(startIndex, array.Count - startIndex);
		if (context.Options.EvaluatingAs.HasFlag(Draft.Draft202012) ||
		    context.Options.EvaluatingAs.HasFlag(Draft.DraftNext) ||
		    context.Options.EvaluatingAs == Draft.Unspecified)
		{
			var evaluatedByContains = context.LocalResult.GetAllAnnotations(ContainsKeyword.Name)
				.SelectMany(x => x!.AsArray().Select(j => j!.GetValue<int>()))
				.Distinct()
				.ToArray();
			if (evaluatedByContains.Any())
			{
				context.Log(() => $"Annotations from {ContainsKeyword.Name}: {annotations.ToJsonArray().AsJsonString()}.");
				indicesToEvaluate = indicesToEvaluate.Except(evaluatedByContains);
			}
			else
				context.Log(() => $"No annotations from {ContainsKeyword.Name}.");
		}
		foreach (var i in indicesToEvaluate)
		{
			context.Log(() => $"Evaluating item at index {i}.");
			var item = array[i];
			context.Push(context.InstanceLocation.Combine(i), item ?? JsonNull.SignalNode,
				context.EvaluationPath.Combine(Name), Schema);
			context.Evaluate();
			overallResult &= context.LocalResult.IsValid;
			context.Log(() => $"Item at index {i} {context.LocalResult.IsValid.GetValidityString()}.");
			context.Pop();
			if (!overallResult && context.ApplyOptimizations) break;
		}
		context.Options.LogIndentLevel--;

		context.LocalResult.SetAnnotation(Name, true);
		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation, EvaluationOptions options)
	{
		IEnumerable<Requirement> GetDynamicRequirements(IEnumerable<int> indices)
		{
			foreach (var i in indices)
			{
				foreach (var requirement in Schema.GenerateRequirements(baseUri, subschemaPath.Combine(Name), instanceLocation.Combine(i), options).InOrder())
				{
					yield return requirement;
				}
			}
		}

		yield return new Requirement(subschemaPath, instanceLocation,
			(node, cache, catalog) =>
			{
				if (node is not JsonArray arr) return null;

				var localAndSubschemaKeywordResults = cache.Where(x => x.SubschemaPath.StartsWith(subschemaPath));
				var groupedByLocalSubschema = localAndSubschemaKeywordResults.GroupBy(x => x.SubschemaPath).ToArray();
				var subschemasThatDroppedAnnotations = groupedByLocalSubschema.Where(x => x.Any(k => k.ValidationResult == false)).Select(x => x.Key);
				var subschemasThatRetainedAnnotations = groupedByLocalSubschema.Where(x => !subschemasThatDroppedAnnotations.Any(p => x.Key.StartsWith(p)) || x.Key == subschemaPath);
				var annotations = subschemasThatRetainedAnnotations.SelectMany(x => x)
					.Where(x => x.Keyword is PrefixItemsKeyword.Name or ItemsKeyword.Name or AdditionalItemsKeyword.Name or ContainsKeyword.Name or Name)
					.Select(x => (x.Keyword, x.Annotation))
					.ToArray();
				if (annotations.Any(x => x.Annotation.IsEquivalentTo(true))) return null;

				var evaluatedIndices = annotations.SelectMany(x =>
				{
					if (x.Keyword is PrefixItemsKeyword.Name or ItemsKeyword.Name)
						return Enumerable.Range(0, x.Annotation!.GetValue<int>() + 1);
					if (x.Keyword == ContainsKeyword.Name)
						return x.Annotation.ToIntArray();
					throw new ArgumentException("Unexpected annotation value");
				});

				var dynamicRequirements = GetDynamicRequirements(Enumerable.Range(0, arr.Count).Except(evaluatedIndices));
				dynamicRequirements.Evaluate(cache, catalog);

				return new KeywordResult(Name, subschemaPath, baseUri, instanceLocation)
				{
					ValidationResult = cache.GetLocalResults(subschemaPath, Name).All(x => x.ValidationResult != false),
					Annotation = true
				};
			}, 30);
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		Schema.RegisterSubschemas(registry, currentUri);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(UnevaluatedItemsKeyword? other)
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
		return Equals(obj as UnevaluatedItemsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Schema.GetHashCode();
	}
}

internal class UnevaluatedItemsKeywordJsonConverter : JsonConverter<UnevaluatedItemsKeyword>
{
	public override UnevaluatedItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;

		return new UnevaluatedItemsKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, UnevaluatedItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(UnevaluatedItemsKeyword.Name);
		JsonSerializer.Serialize(writer, value.Schema, options);
	}
}