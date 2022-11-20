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
[Applicator]
[SchemaPriority(10)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[JsonConverter(typeof(AdditionalItemsKeywordJsonConverter))]
public class AdditionalItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<AdditionalItemsKeyword>
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

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation, EvaluationOptions options)
	{
		IEnumerable<Requirement> GetDynamicRequirements(int startIndex, int itemCount)
		{
			for (var i = startIndex; i < itemCount; i++)
			{
				foreach (var requirement in Schema.GenerateRequirements(baseUri, subschemaPath.Combine(Name), instanceLocation.Combine(i), options))
				{
					yield return requirement;
				}
			}
		}

		yield return new Requirement(subschemaPath, instanceLocation,
			(node, cache, catalog) =>
			{
				if (node is not JsonArray arr) return null;

				var itemsResults = cache.SingleOrDefault(x => x.SubschemaPath == subschemaPath && x.Keyword == ItemsKeyword.Name);
				var itemsAnnotation = itemsResults?.Annotation!.AsValue();
				int lastIndex = 0;
				// don't need to check the boolean as it's only going to be true
				if (itemsAnnotation != null && itemsAnnotation.TryGetValue<bool>(out _)) return null;

				itemsAnnotation?.TryGetValue(out lastIndex);
				
				var dynamicRequirements = GetDynamicRequirements(lastIndex, arr.Count);
				dynamicRequirements.Evaluate(cache, catalog);

				return new KeywordResult(Name, subschemaPath, baseUri, instanceLocation)
				{
					ValidationResult = cache.GetLocalResults(subschemaPath, Name).All(x => x.ValidationResult != false),
					Annotation = true
				};
			}, 10);
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		Schema.RegisterSubschemas(registry, currentUri);
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