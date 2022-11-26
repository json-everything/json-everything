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
/// Handles `items`.
/// </summary>
[Applicator]
[SchemaPriority(5)]
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[JsonConverter(typeof(ItemsKeywordJsonConverter))]
public class ItemsKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, ISchemaCollector, IEquatable<ItemsKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "items";

	/// <summary>
	/// The schema for the "single schema" form.
	/// </summary>
	public JsonSchema? SingleSchema { get; }

	JsonSchema ISchemaContainer.Schema => SingleSchema!;

	/// <summary>
	/// The collection of schemas for the "schema array" form.
	/// </summary>
	public IReadOnlyList<JsonSchema>? ArraySchemas { get; }

	IReadOnlyList<JsonSchema> ISchemaCollector.Schemas => ArraySchemas!;

	/// <summary>
	/// Creates a new <see cref="ItemsKeyword"/>.
	/// </summary>
	/// <param name="value">The schema for the "single schema" form.</param>
	public ItemsKeyword(JsonSchema value)
	{
		SingleSchema = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Creates a new <see cref="ItemsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schemas for the "schema array" form.</param>
	/// <remarks>
	/// Using the `params` constructor to build an array-form `items` keyword with a single schema
	/// will confuse the compiler.  To achieve this, you'll need to explicitly specify the array.
	/// </remarks>
	public ItemsKeyword(params JsonSchema[] values)
	{
		ArraySchemas = values.ToReadOnlyList();
	}

	/// <summary>
	/// Creates a new <see cref="ItemsKeyword"/>.
	/// </summary>
	/// <param name="values">The collection of schemas for the "schema array" form.</param>
	public ItemsKeyword(IEnumerable<JsonSchema> values)
	{
		ArraySchemas = values.ToReadOnlyList();
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

		var array = (JsonArray)context.LocalInstance!;
		var overallResult = true;
		if (SingleSchema != null)
		{
			context.Options.LogIndentLevel++;
			int startIndex;
			if (!context.LocalResult.TryGetAnnotation(PrefixItemsKeyword.Name, out var annotation))
				startIndex = 0;
			else
			{
				context.Log(() => $"Annotation from {PrefixItemsKeyword.Name}: {annotation.AsJsonString()}");
				if (annotation!.AsValue().TryGetValue(out bool _))
				{
					context.ExitKeyword(Name, true);
					return;
				}

				startIndex = (int)annotation;
			}

			for (int i = startIndex; i < array.Count; i++)
			{
				var i1 = i;
				context.Log(() => $"Evaluating item at index {i1}.");
				var item = array[i];
				context.Push(context.InstanceLocation.Combine(i), item ?? JsonNull.SignalNode,
					context.EvaluationPath.Combine(Name), SingleSchema);
				context.Evaluate();
				overallResult &= context.LocalResult.IsValid;
				context.Log(() => $"Item at index {i1} {context.LocalResult.IsValid.GetValidityString()}.");
				context.Pop();
				if (!overallResult && context.ApplyOptimizations) break;
			}
			context.Options.LogIndentLevel--;

			context.LocalResult.SetAnnotation(Name, true);
		}
		else // array
		{
			if (context.Options.EvaluatingAs.HasFlag(Draft.Draft202012) ||
			    context.Options.EvaluatingAs.HasFlag(Draft.DraftNext))
			{
				context.LocalResult.Fail(Name, ErrorMessages.InvalidItemsForm);
				context.Log(() => $"Array form of {Name} is invalid for draft 2020-12 and later");
				context.ExitKeyword(Name, false);
				return;
			}
			context.Options.LogIndentLevel++;
			var maxEvaluations = Math.Min(ArraySchemas!.Count, array.Count);
			for (int i = 0; i < maxEvaluations; i++)
			{
				var i1 = i;
				context.Log(() => $"Evaluating item at index {i1}.");
				var schema = ArraySchemas[i];
				var item = array[i];
				context.Push(context.InstanceLocation.Combine(i),
					item ?? JsonNull.SignalNode,
					context.EvaluationPath.Combine(i),
					schema);
				context.Evaluate();
				overallResult &= context.LocalResult.IsValid;
				context.Log(() => $"Item at index {i1} {context.LocalResult.IsValid.GetValidityString()}.");
				context.Pop();
				if (!overallResult && context.ApplyOptimizations) break;
			}
			context.Options.LogIndentLevel--;

			if (maxEvaluations == array.Count)
				context.LocalResult.SetAnnotation(Name, true);
			else
				context.LocalResult.SetAnnotation(Name, maxEvaluations);
		}

		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, DynamicScope scope, JsonPointer instanceLocation, EvaluationOptions options)
	{
		IEnumerable<Requirement> GetDynamicRequirements(int startIndex, int itemCount)
		{
			for (var i = startIndex; i < itemCount; i++)
			{
				foreach (var requirement in SingleSchema.GenerateRequirements(scope, subschemaPath.Combine(Name), instanceLocation.Combine(i), options))
				{
					yield return requirement;
				}
			}
		}

		if (SingleSchema != null)
		{
			yield return new Requirement(subschemaPath, instanceLocation,
				(node, cache, catalog) =>
				{
					if (node is not JsonArray arr) return null;

					var prefixItemsAnnotation = cache.GetLocalAnnotation(subschemaPath, PrefixItemsKeyword.Name)?.AsValue();
					// don't need to check the boolean as it's only going to be true if it exists
					int lastIndex = -1;
					if (prefixItemsAnnotation != null)
					{
						if (prefixItemsAnnotation.TryGetValue<bool>(out _)) return null;
						lastIndex = prefixItemsAnnotation.GetValue<int>();
					}

					var dynamicRequirements = GetDynamicRequirements(lastIndex + 1, arr.Count);
					dynamicRequirements.Evaluate(cache, catalog);

					return new KeywordResult(Name, subschemaPath, scope.LocalScope, instanceLocation)
					{
						ValidationResult = cache.GetLocalResults(subschemaPath, Name).All(x => x.ValidationResult != false),
						Annotation = true
					};
				}, 20);
		}
		else
		{
			for (int i = 0; i < ArraySchemas!.Count; i++)
			{
				var schema = ArraySchemas[i];
				foreach (var requirement in schema.GenerateRequirements(scope, subschemaPath.Combine(Name, i), instanceLocation.Combine(i), options))
				{
					yield return requirement;
				}
			}

			yield return new Requirement(subschemaPath, instanceLocation,
				(node, cache, _) =>
				{
					if (node is not JsonArray arr) return null;

					var itemCount = Math.Min(arr.Count, ArraySchemas.Count);

					var relevantEvaluationPaths = Enumerable.Range(0, itemCount).Select(k => subschemaPath.Combine(Name, k));
					var relevantResults = cache.Where(x => relevantEvaluationPaths.Contains(x.SubschemaPath)).ToList();

					var validCount = relevantResults.TakeWhile(x => x.ValidationResult != false).Count();

					return new KeywordResult(Name, subschemaPath, scope.LocalScope, instanceLocation)
					{
						ValidationResult = itemCount == validCount,
						Annotation = arr.Count == validCount ? true : validCount - 1
					};
				});
		}
	}

	void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
	{
		if (SingleSchema != null)
			SingleSchema.RegisterSubschemas(registry, currentUri);
		else
		{
			foreach (var schema in ArraySchemas!)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(ItemsKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (SingleSchema != null)
		{
			if (other.SingleSchema == null) return false;
			return Equals(SingleSchema, other.SingleSchema);
		}

		if (ArraySchemas != null)
		{
			if (other.ArraySchemas == null) return false;
			return ArraySchemas.ContentsEqual(other.ArraySchemas);
		}

		throw new InvalidOperationException("Either SingleSchema or ArraySchemas should be populated.");
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as ItemsKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = SingleSchema?.GetHashCode() ?? 0;
			hashCode = (hashCode * 397) ^ (ArraySchemas?.GetUnorderedCollectionHashCode() ?? 0);
			return hashCode;
		}
	}
}

internal class ItemsKeywordJsonConverter : JsonConverter<ItemsKeyword>
{
	public override ItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.StartArray)
		{
			var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options)!;
			return new ItemsKeyword(schemas);
		}

		var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)!;
		return new ItemsKeyword(schema);
	}
	public override void Write(Utf8JsonWriter writer, ItemsKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(ItemsKeyword.Name);
		if (value.SingleSchema != null)
			JsonSerializer.Serialize(writer, value.SingleSchema, options);
		else
		{
			writer.WriteStartArray();
			foreach (var schema in value.ArraySchemas!)
			{
				JsonSerializer.Serialize(writer, schema, options);
			}
			writer.WriteEndArray();
		}
	}
}

public static partial class ErrorMessages
{
	private static string? _invalidItemsForm;

	/// <summary>
	/// Gets or sets the error message for when <see cref="ItemsKeyword"/> is specified
	/// with an array of schemas in a draft 2020-12 or later schema.
	/// </summary>
	/// <remarks>No tokens are supported.</remarks>
	public static string InvalidItemsForm
	{
		get => _invalidItemsForm ?? Get();
		set => _invalidItemsForm = value;
	}
}