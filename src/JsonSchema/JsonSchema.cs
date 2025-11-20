using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;

// ReSharper disable LocalizableElement

namespace Json.Schema;

/// <summary>
/// Represents a JSON Schema.
/// </summary>
[DebuggerDisplay("{ToDebugString()}")]
public class JsonSchema : IBaseDocument
{
	private const string _unknownKeywordsAnnotationKey = "$unknownKeywords";

	private readonly Dictionary<string, KeywordData>? _keywords;
	private BuildOptions _options;
	private JsonSchemaNode _root;

	/// <summary>
	/// The `true` schema.  Passes all instances.
	/// </summary>
	public static readonly JsonSchema True = new(true) { BaseUri = new("https://json-schema.org/true") };

	/// <summary>
	/// The `false` schema.  Fails all instances.
	/// </summary>
	public static readonly JsonSchema False = new(false) { BaseUri = new("https://json-schema.org/false") };

	/// <summary>
	/// For boolean schemas, gets the value.  Null if the schema isn't a boolean schema.
	/// </summary>
	public bool? BoolValue { get; }

	/// <summary>
	/// Gets the base URI that applies to this schema.  This may be defined by a parent schema.
	/// </summary>
	/// <remarks>
	/// This property is initialized to a generated random value that matches `https://json-everything.net/{random}`
	/// where `random` is 10 hex characters.
	///
	/// It may change after the initial evaluation based on whether the schema contains an `$id` keyword
	/// or is a child of another schema.
	/// </remarks>
	public Uri BaseUri { get; set; } = GenerateBaseUri();

	/// <summary>
	/// Gets whether the schema defines a new schema resource.  This will only be true if it contains an `$id` keyword.
	/// </summary>
	public bool IsResourceRoot { get; private set; }

	private JsonSchema(bool value)
	{
		BoolValue = value;
	}

	private JsonSchema(JsonSchemaNode root, BuildOptions options)
	{
		_root = root;
		_options = options;

		//options.SchemaRegistry.Register(this);
	}

	/// <summary>
	/// Loads text from a file and deserializes a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="fileName">The filename to load, URL-decoded.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	/// <remarks>The filename needs to not be URL-encoded as <see cref="Uri"/> attempts to encode it.</remarks>
	public static JsonSchema FromFile(string fileName, JsonSerializerOptions? options = null)
	{
		var text = File.ReadAllText(fileName);
		var schema = FromText(text, options);
		var path = Path.GetFullPath(fileName);
		// For some reason, full *nix file paths (which start with '/') don't work quite right when
		// being prepended with 'file:///'.  It seems the '////' is interpreted as '//' and the
		// first folder in the path is then interpreted as the host.  To account for this, we
		// need to prepend with 'file://' instead.
		var protocol = path.StartsWith("/") ? "file://" : "file:///";
		schema.BaseUri = new Uri($"{protocol}{path}");
		return schema;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from text.
	/// </summary>
	/// <param name="jsonText">The text to parse.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	public static JsonSchema FromText(string jsonText, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.Deserialize<JsonSchema>(jsonText, options)!;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from a stream.
	/// </summary>
	/// <param name="source">A stream.</param>
	/// <param name="options">Serializer options.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	public static ValueTask<JsonSchema> FromStream(Stream source, JsonSerializerOptions? options = null)
	{
		return JsonSerializer.DeserializeAsync<JsonSchema>(source, options)!;
	}

	private static Uri GenerateBaseUri() => new($"https://json-everything.net/{Guid.NewGuid().ToString("N")[..10]}");

	public static JsonSchema Build(JsonElement root, BuildOptions? options = null)
	{
		if (root.ValueKind == JsonValueKind.True) return True;
		if (root.ValueKind == JsonValueKind.False) return False;

		if (root.ValueKind != JsonValueKind.Object)
			throw new ArgumentException($"Schemas may only booleans or objects.  Received {root.ValueKind}");

		var context = new BuildContext(options, root, GenerateBaseUri())
		{
			LocalSchema = root
		};

		var node = BuildNode(context);

		var schema = new JsonSchema(node, context.Options){BaseUri = context.BaseUri};
		context.Options.SchemaRegistry.Register(schema);

		TryResolveReferences(node, context);

		return schema;
	}

	public static JsonSchemaNode BuildNode(BuildContext context)
	{
		// TODO: for consideration: resolving references as part of the build might prevent support of cyclical or recursive references.  A <--> B
		//       how do we register B for A to reference without first building B which needs A to be registered?
		//       - register A - try-resolve refs misses B
		//       - register B - try-resolve refs finds A
		//                    - recursive try-resolve into A finds B

		var node = new JsonSchemaNode
		{
			BaseUri = context.BaseUri,
			Source = context.LocalSchema
		};
	
		var keywordData = new List<KeywordData>();
		foreach (var property in context.LocalSchema.EnumerateObject())
		{
			var keyword = property.Name;
			var value = property.Value;

			var handler = context.Options.KeywordRegistry.GetHandler(keyword);
			if (handler is null) continue; // TODO: for v1, throw exception if not x-*

			var data = new KeywordData
			{
				EvaluationOrder = context.Options.KeywordRegistry.GetEvaluationOrder(keyword) ??
				                  throw new UnreachableException("Cannot get evaluation order for keyword"),
				RawValue = value.Clone(),
				Handler = handler,
				Value = handler.ValidateValue(value)
			};
			handler.BuildSubschemas(data, context);

			keywordData.Add(data);
		}

		node.Keywords = keywordData.OrderBy(x => x.EvaluationOrder).ToArray();

		return node;
	}

	public static bool TryResolveReferences(JsonSchemaNode node, BuildContext context)
	{
		var allResolved = true;
		var refKeyword = node.Keywords.SingleOrDefault(x => x.Handler is RefKeyword);
		if (refKeyword is not null)
		{
			var handler = (RefKeyword)refKeyword.Handler;
			allResolved &= handler.Resolve(refKeyword, context);
		}

		foreach (var keyword in node.Keywords)
		{
			foreach (var subNode in keyword.Subschemas)
			{
				allResolved &= TryResolveReferences(subNode, context);
			}
		}

		return allResolved;
	}

	public EvaluationResults Evaluate(JsonElement instance, EvaluationOptions? options = null)
	{
		options ??= EvaluationOptions.Default;
		var context = new EvaluationContext
		{
			Options = options,
			Instance = instance
		};

		return _root.Evaluate(context);
	}

	public JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildOptions options)
	{
		var subschema = _root;
		while (pointer.SegmentCount != 0)
		{
			var keyword = subschema.Keywords.FirstOrDefault(x => pointer.StartsWith(JsonPointer.Create(x.Handler.Name)));
			if (keyword is null) return null;

			pointer = pointer.GetLocal(pointer.SegmentCount - 1);

			subschema = keyword.Subschemas.FirstOrDefault(x => pointer.StartsWith(x.RelativePath));
			if (subschema is null) return null;

			pointer = pointer.GetLocal(pointer.SegmentCount - subschema.RelativePath.SegmentCount);
		}

		return subschema;
	}

	/// <summary>
	/// Implicitly converts a boolean value into one of the boolean schemas. 
	/// </summary>
	/// <param name="value">The boolean value.</param>
	public static implicit operator JsonSchema(bool value)
	{
		return value ? True : False;
	}

	private string ToDebugString()
	{
		if (BoolValue.HasValue) return BoolValue.Value ? "true" : "false";

		var idKeyword = _keywords!.SingleOrDefault(x => x.Value.Handler is IdKeyword);
		return idKeyword.Value?.RawValue.GetString() ?? BaseUri.OriginalString;
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for the "false" schema.
	/// </summary>
	/// <remarks>No tokens are supported.</remarks>
	public static string FalseSchema
	{
		get => field ?? Get();
		set;
	}
}

public interface IKeywordHandler
{
	string Name { get; }

	object? ValidateValue(JsonElement value);
	void BuildSubschemas(KeywordData keyword, BuildContext context);
	KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context);
}

public class JsonSchemaNode
{
	public required Uri BaseUri { get; set; }
	public JsonElement Source { get; set; }
	public KeywordData[] Keywords { get; set; } = [];
	public JsonPointer RelativePath { get; set; }

	public EvaluationResults Evaluate(EvaluationContext context)
	{
		var keywordEvaluations = Keywords.Select(x => x.Handler.Evaluate(x, context)).ToArray();

		var results = new EvaluationResults(context.EvaluationPath, BaseUri, context.InstanceLocation, context.Options);
		if (!keywordEvaluations.All(x => x.IsValid))
			results.IsValid = false;

		foreach (var evaluation in keywordEvaluations)
		{
			if (evaluation.Details is null) continue;

			foreach (var nestedResult in evaluation.Details)
			{
				results.Details ??= [];
				results.Details.Add(nestedResult);
			}
		}

		foreach (var evaluation in keywordEvaluations)
		{
			if (evaluation.Error is null) continue;

			results.Errors ??= [];
			results.Errors[evaluation.Keyword] = evaluation.Error!;
		}

		return results;
	}
}

public class KeywordData
{
	public required long EvaluationOrder { get; set; }
	public required IKeywordHandler Handler { get; set; }
	public required JsonElement RawValue { get; set; }
	public JsonSchemaNode[] Subschemas { get; set; } = [];
	public object? Value { get; set; }
}

public readonly struct KeywordEvaluation
{
	public static KeywordEvaluation Ignore = new()
	{
		Keyword = Guid.NewGuid().ToString("N"),
		IsValid = true
	};

	public required string Keyword { get; init; }
	public required bool IsValid { get; init; }
	public string[]? Annotations { get; init; }
	public EvaluationResults[]? Details { get; init; }
	public string? Error { get; init; }

	public KeywordEvaluation(){}
}

public struct BuildContext
{
	private readonly Stack<(string, JsonPointer)> _navigatedReferences = [];

	public BuildOptions Options { get; }
	public JsonElement RootSchema { get; }
	public Uri BaseUri { get; set; }
	public JsonElement LocalSchema { get; set; }

	internal BuildContext(BuildOptions? options, JsonElement rootSchema, Uri baseUri)
	{
		Options = options ?? BuildOptions.Default;
		RootSchema = rootSchema;
		BaseUri = baseUri;
	}

	public void PushNavigation(string uri, JsonPointer instanceLocation)
	{
		var value = (uri, instanceLocation);
		if (_navigatedReferences.Contains(value))
			throw new JsonSchemaException($"Encountered circular reference at schema location `{uri}` and instance location `{instanceLocation}`");

		_navigatedReferences.Push(value);
	}

	public void PopNavigation()
	{
		_navigatedReferences.Pop();
	}
}