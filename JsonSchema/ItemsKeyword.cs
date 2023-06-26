using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `items`.
/// </summary>
[SchemaPriority(5)]
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(PrefixItemsKeyword))]
[JsonConverter(typeof(ItemsKeywordJsonConverter))]
public class ItemsKeyword : IJsonSchemaKeyword, ISchemaContainer, ISchemaCollector, IEquatable<ItemsKeyword>
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
	/// <param name="token">The cancellation token used by the caller.</param>
	public async Task Evaluate(EvaluationContext context, CancellationToken token)
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

		var tokenSource = new CancellationTokenSource();
		token.Register(tokenSource.Cancel);
		Task<bool>[] tasks;
		var maxEvaluations = array.Count;

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

			tasks = Enumerable.Range(startIndex, array.Count - startIndex)
				.Select(i => Task.Run(async () =>
				{
					if (tokenSource.Token.IsCancellationRequested) return true;

					context.Log(() => $"Evaluating item at index {i}.");
					var item = array[i];
					var branch = context.ParallelBranch(context.InstanceLocation.Combine(i), item ?? JsonNull.SignalNode,
						context.EvaluationPath.Combine(Name), SingleSchema);
					await branch.Evaluate(tokenSource.Token);
					context.Log(() => $"Item at index {i} {branch.LocalResult.IsValid.GetValidityString()}.");

					return branch.LocalResult.IsValid;
				}, tokenSource.Token)).ToArray();
		}
		else // array
		{
			if (context.Options.EvaluatingAs.HasFlag(SpecVersion.Draft202012) ||
			    context.Options.EvaluatingAs.HasFlag(SpecVersion.DraftNext))
				throw new JsonSchemaException($"Array form of {Name} is invalid for draft 2020-12 and later");

			context.Options.LogIndentLevel++;
			maxEvaluations = Math.Min(ArraySchemas!.Count, array.Count);

			tasks = Enumerable.Range(0, maxEvaluations)
				.Select(i => Task.Run(async () =>
				{
					if (tokenSource.Token.IsCancellationRequested) return true;

					context.Log(() => $"Evaluating item at index {i}.");
					var schema = ArraySchemas[i];
					var item = array[i];
					var branch = context.ParallelBranch(context.InstanceLocation.Combine(i),
						item ?? JsonNull.SignalNode,
						context.EvaluationPath.Combine(i),
						schema);
					await branch.Evaluate(tokenSource.Token);
					context.Log(() => $"Item at index {i} {branch.LocalResult.IsValid.GetValidityString()}.");

					return branch.LocalResult.IsValid;
				}, tokenSource.Token)).ToArray();
		}

		if (tasks.Any())
		{
			if (context.ApplyOptimizations)
			{
				var failedValidation = await tasks.WhenAny(x => !x, tokenSource.Token);
				tokenSource.Cancel();

				overallResult = failedValidation == null;
			}
			else
			{
				await Task.WhenAll(tasks);
				overallResult = tasks.All(x => x.Result);
			}
		}


		context.Options.LogIndentLevel--;
		if (maxEvaluations == array.Count)
			context.LocalResult.SetAnnotation(Name, true);
		else
			context.LocalResult.SetAnnotation(Name, maxEvaluations);

		if (!overallResult)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
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