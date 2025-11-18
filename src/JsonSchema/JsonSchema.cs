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
	private JsonElement _source;
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

	private JsonSchema(JsonElement source, JsonSchemaNode root, BuildOptions options)
	{
		_source = source;
		_root = root;
		_options = options;

		options.SchemaRegistry.Register(this);
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

		var node = new JsonSchemaNode();
		var context = new BuildContext(options);
		
		foreach (var property in root.EnumerateObject())
		{
			var keyword = property.Name;
			var value = property.Value;

			var handler = context.Options.KeywordRegistry.GetHandler(keyword);
			if (handler is null) continue; // TODO: for v1, throw exception if not x-*

			node.Keywords[keyword] = new KeywordData
			{
				RawValue = value.Clone(),
				Value = handler.ValidateValue(value),
				Subschemas = handler.BuildSubschemas(context)
			};
		}

		return new JsonSchema(root, node, context.Options);
	}

	public EvaluationResults Evaluate(JsonElement? root, EvaluationOptions? options = null)
	{
		options ??= EvaluationOptions.Default;

		throw new NotImplementedException();
	}

	public JsonSchema? FindSubschema(JsonPointer pointer, EvaluationOptions options)
	{
		throw new NotImplementedException();
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

		var idKeyword = _keywords.Where(x => x.Value.Handler is IdKeyword).SingleOrDefault();
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
	JsonSchemaNode[] BuildSubschemas(BuildContext context);
	KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context);
}

public class JsonSchemaNode
{
	public Dictionary<string, KeywordData> Keywords { get; set; } = [];
}

public class KeywordData
{
	public int EvaluationOrder { get; set; }
	public JsonElement RawValue { get; set; }
	public object? Value { get; set; }
	public JsonSchemaNode[] Subschemas { get; set; }
	public IKeywordHandler Handler { get; set; }
}

public readonly struct KeywordEvaluation
{
	public required string Keyword { get; init; }
	public required bool IsValid { get; init; }
	public string[] Annotations { get; init; } = [];

	public KeywordEvaluation(){}
}

public struct BuildContext(BuildOptions? options)
{
	public BuildOptions Options { get; } = options ?? BuildOptions.Default;
	public JsonPointer EvaluationPath { get; set; }
	public JsonElement LocalSchema { get; set; }
}