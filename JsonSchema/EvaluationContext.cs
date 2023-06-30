using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides a single source of data for evaluation operations.
/// </summary>
[JsonConverter(typeof(DebugContextConverter))]
public class EvaluationContext
{
	private readonly Stack<JsonNode?> _localInstances = new();
	private readonly Stack<JsonPointer> _instanceLocations = new();
	private readonly Stack<JsonSchema> _localSchemas = new();
	private readonly Stack<JsonPointer> _evaluationPaths = new();
	private readonly Stack<EvaluationResults> _localResults = new();
	private readonly Stack<IReadOnlyDictionary<Uri, bool>?> _metaSchemaVocabs = new();
	private readonly Stack<bool> _requireAnnotations = new();

	/// <summary>
	/// The option set for the evaluation.
	/// </summary>
	public EvaluationOptions Options { get; }

	/// <summary>
	/// The root schema.
	/// </summary>
	public JsonSchema SchemaRoot { get; }

	/// <summary>
	/// The current subschema location relative to the schema root.
	/// </summary>
	public JsonPointer EvaluationPath => _evaluationPaths.Peek();

	/// <summary>
	/// The current subschema.
	/// </summary>
	public JsonSchema LocalSchema => _localSchemas.Peek();
	/// <summary>
	/// The instance root.
	/// </summary>
	public JsonNode? InstanceRoot { get; }

	/// <summary>
	/// The current instance location relative to the instance root.
	/// </summary>
	public JsonPointer InstanceLocation => _instanceLocations.Peek();

	/// <summary>
	/// The current instance.
	/// </summary>
	public JsonNode? LocalInstance => _localInstances.Peek();

	/// <summary>
	/// Gets the scope of the current evaluation.
	/// </summary>
	public DynamicScope Scope { get; }

	/// <summary>
	/// The result object for the current evaluation.
	/// </summary>
	public EvaluationResults LocalResult => _localResults.Peek();

	internal IReadOnlyDictionary<Uri, bool>? MetaSchemaVocabs => _metaSchemaVocabs.Peek();

	internal List<(string, JsonPointer)> NavigatedReferences { get; } = new();

	/// <summary>
	/// Whether processing optimizations can be applied (output format = flag).
	/// </summary>
	public bool ApplyOptimizations => Options.OutputFormat == OutputFormat.Flag && !_requireAnnotations.Peek();

	internal EvaluationContext(EvaluationOptions options,
		Uri currentUri,
		JsonNode? instanceRoot,
		JsonSchema schemaRoot)
	{
		Options = options;
		InstanceRoot = instanceRoot;
		SchemaRoot = schemaRoot;
		Scope = new DynamicScope(currentUri);
		_localInstances.Push(instanceRoot);
		_instanceLocations.Push(JsonPointer.Empty);
		_localSchemas.Push(schemaRoot);
		_evaluationPaths.Push(JsonPointer.Empty);
		_localResults.Push(new EvaluationResults(this));
		_metaSchemaVocabs.Push(null);
		_requireAnnotations.Push(RequiresAnnotationCollection(schemaRoot));
	}

	private EvaluationContext(EvaluationContext source)
	{
		Options = source.Options;
		InstanceRoot = source.InstanceRoot;
		SchemaRoot = source.SchemaRoot;
		Scope = new DynamicScope(source.Scope);
		_localInstances.Push(source.LocalInstance);
		_instanceLocations.Push(source.InstanceLocation);
		_localSchemas.Push(source.LocalSchema);
		_evaluationPaths.Push(source.EvaluationPath);
		_localResults.Push(source.LocalResult);
		_metaSchemaVocabs.Push(source.MetaSchemaVocabs);
		_requireAnnotations.Push(source._requireAnnotations.Peek());
	}

	/// <summary>
	/// Creates a new context to be used for parallel processing.
	/// </summary>
	/// <param name="instanceLocation">The location within the data instance root.</param>
	/// <param name="instance">The data instance.</param>
	/// <param name="evaluationPath">The location within the schema root.</param>
	/// <param name="subschema">The subschema.</param>
	/// <returns>A new context.</returns>
	public EvaluationContext ParallelBranch(in JsonPointer instanceLocation,
		in JsonNode? instance,
		in JsonPointer evaluationPath,
		in JsonSchema subschema)
	{
		var branch = new EvaluationContext(this);
		branch.Push(instanceLocation, instance, evaluationPath, subschema);

		return branch;
	}

	/// <summary>
	/// Creates a new context to be used for parallel processing.
	/// </summary>
	/// <param name="evaluationPath">The location within the schema root.</param>
	/// <param name="subschema">The subschema.</param>
	/// <returns>A new context.</returns>
	public EvaluationContext ParallelBranch(in JsonPointer evaluationPath,
		in JsonSchema subschema)
	{
		var branch = new EvaluationContext(this);
		branch.Push(evaluationPath, subschema);

		return branch;
	}

	/// <summary>
	/// Pushes the state onto the stack and sets up for a nested layer of evaluation.
	/// </summary>
	/// <param name="instanceLocation">The location within the data instance root.</param>
	/// <param name="instance">The data instance.</param>
	/// <param name="evaluationPath">The location within the schema root.</param>
	/// <param name="subschema">The subschema.</param>
	public void Push(in JsonPointer instanceLocation,
		in JsonNode? instance,
		in JsonPointer evaluationPath,
		in JsonSchema subschema)
	{
		_instanceLocations.Push(instanceLocation);
		_localInstances.Push(instance);
		_evaluationPaths.Push(evaluationPath);
		_localSchemas.Push(subschema);
		_requireAnnotations.Push(_requireAnnotations.Peek() || RequiresAnnotationCollection(subschema));
		var newResult = new EvaluationResults(this);
		LocalResult.AddNestedResult(newResult);
		_localResults.Push(newResult);
		_metaSchemaVocabs.Push(_metaSchemaVocabs.Peek());
		if (Scope.LocalScope != subschema.BaseUri)
			Scope.Push(subschema.BaseUri);
	}

	/// <summary>
	/// Pushes the state onto the stack and sets up for a nested layer of evaluation.
	/// </summary>
	/// <param name="evaluationPath">The location within the schema root.</param>
	/// <param name="subschema">The subschema.</param>
	public void Push(in JsonPointer evaluationPath,
		in JsonSchema subschema)
	{
		_instanceLocations.Push(InstanceLocation);
		_localInstances.Push(LocalInstance);
		_evaluationPaths.Push(evaluationPath);
		_localSchemas.Push(subschema);
		_requireAnnotations.Push(_requireAnnotations.Peek() || RequiresAnnotationCollection(subschema));
		var newResult = new EvaluationResults(this);
		LocalResult.AddNestedResult(newResult);
		_localResults.Push(newResult);
		_metaSchemaVocabs.Push(_metaSchemaVocabs.Peek());
		if (Scope.LocalScope != subschema.BaseUri)
			Scope.Push(subschema.BaseUri);
	}

	/// <summary>
	/// Evaluates as a subschema.  To be called from within keywords.
	/// </summary>
	/// <param name="token">The cancellation token used by the caller.</param>
	public async Task Evaluate(CancellationToken token)
	{
		if (LocalSchema.BoolValue.HasValue)
		{
			this.Log(() => $"Found {(LocalSchema.BoolValue.Value ? "true" : "false")} schema: {LocalSchema.BoolValue.Value.GetValidityString()}");
			if (!LocalSchema.BoolValue.Value)
				LocalResult.Fail(string.Empty, ErrorMessages.FalseSchema);
			return;
		}

		var keywords = Options.FilterKeywords(LocalSchema.Keywords!, LocalSchema.DeclaredVersion).ToArray();

		var schemaKeyword = keywords.OfType<SchemaKeyword>().SingleOrDefault();
		if (schemaKeyword != null)
			await schemaKeyword.Evaluate(this, token);

		var keywordTypesToProcess = GetKeywordsToProcess();

		var filteredAndGrouped = keywords.GroupBy(x => x.Priority())
			.OrderBy(x => x.Key);

		foreach (var group in filteredAndGrouped)
		{
			if (token.IsCancellationRequested) return;

			var tokenSource = new CancellationTokenSource();
			token.Register(tokenSource.Cancel);

			var processable = group.Where(x => Options.ProcessCustomKeywords || (keywordTypesToProcess?.Contains(x.GetType()) ?? true));

			var tasks = processable.Select(x =>
			{
				if (!tokenSource.Token.IsCancellationRequested)
				{
					// I have no idea why this works, but without this, it seems that either the local
					// instance or the instance root just randomly lose their data in a multi-threaded context.
					// Simply touching the local instance at all before entering a secondary thread seems
					// to stop the problem.  I'm leaving this in here for now, but feel free to comment this line
					// and see the chaos that ensues.
					Touch(LocalInstance);

					var branch = new EvaluationContext(this);
					// ReSharper disable once MethodSupportsCancellation
					// We don't want to pass the token into Run() because it would throw a TaskCancelledException
					return Task.Run(async () =>
					{
						await x.Evaluate(branch, tokenSource.Token);
						return branch.LocalResult.IsValid;
					}, tokenSource.Token);
				}
				return Task.FromResult(LocalResult.IsValid);
			});

			if (ApplyOptimizations)
			{
				await tasks.WhenAny(x => !x, tokenSource.Token);
				tokenSource.Cancel();
			}
			else
			{
				await Task.WhenAll(tasks);
			}
		}
	}

	private static void Touch(JsonNode? node)
	{
		// This is the least processor-intensive way I could think to touch the node.
		if (node is JsonObject obj)
			_ = obj.Count;
		else if (node is JsonArray arr)
			_ = arr.Count;
	}

	private static bool RequiresAnnotationCollection(JsonSchema schema)
	{
		return schema.TryGetKeyword<UnevaluatedPropertiesKeyword>(UnevaluatedPropertiesKeyword.Name, out _) ||
		       schema.TryGetKeyword<UnevaluatedItemsKeyword>(UnevaluatedItemsKeyword.Name, out _);
	}

	/// <summary>
	/// Pops the state from the stack to return to a previous layer of evaluation.
	/// </summary>
	public void Pop()
	{
		_instanceLocations.Pop();
		_localInstances.Pop();
		_evaluationPaths.Pop();
		var oldLocalSchema = _localSchemas.Pop();
		_localResults.Pop();
		_metaSchemaVocabs.Pop();
		if (oldLocalSchema.BaseUri != _localSchemas.Peek().BaseUri)
			Scope.Pop();
	}

	internal void UpdateMetaSchemaVocabs(IReadOnlyDictionary<Uri, bool> newVocabSet)
	{
		UpdateCurrentValue(_metaSchemaVocabs, newVocabSet);
	}

	private static void UpdateCurrentValue<T>(Stack<T> stack, T newValue)
	{
		stack.Pop();
		stack.Push(newValue);
	}

	private HashSet<Type>? GetKeywordsToProcess()
	{
		return MetaSchemaVocabs == null
			? null
			: new HashSet<Type>(MetaSchemaVocabs.Keys
				.SelectMany(x => Options.VocabularyRegistry.Get(x)?.Keywords ??
								 Enumerable.Empty<Type>()));
	}
}

internal class DebugContextConverter : JsonConverter<EvaluationContext>
{
	public override EvaluationContext? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, EvaluationContext value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("localInstance");
		JsonSerializer.Serialize(writer, value.LocalInstance, options);
		writer.WritePropertyName("evaluationPath");
		JsonSerializer.Serialize(writer, value.EvaluationPath, options);
		writer.WritePropertyName("localSchema");
		JsonSerializer.Serialize(writer, value.LocalSchema, options);
		writer.WritePropertyName("instanceRoot");
		JsonSerializer.Serialize(writer, value.InstanceRoot, options);
		writer.WritePropertyName("instanceLocation");
		JsonSerializer.Serialize(writer, value.InstanceLocation, options);
		writer.WriteEndObject();
	}
}