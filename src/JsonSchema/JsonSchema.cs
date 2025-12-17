using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;
using Json.Schema.Keywords;

// ReSharper disable LocalizableElement

namespace Json.Schema;

/// <summary>
/// Represents a JSON Schema.
/// </summary>
[DebuggerDisplay("{ToDebugString()}")]
[JsonConverter(typeof(JsonSchemaJsonConverter))]
public class JsonSchema : IBaseDocument
{
	private readonly BuildOptions _buildOptions;
	private readonly bool _refIgnoresSiblingKeywords;
	private bool _resolved;
	private Dictionary<JsonPointer, JsonSchemaNode>? _discoveredSchemas;

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

	/// <remarks>
	/// This property is initialized to a generated random value that matches `https://json-everything.lib/{random-guid}`.
	///
	/// It may change after the initial evaluation based on whether the schema contains an `$id` keyword
	/// or is a child of another schema.
	/// </remarks>
	public Uri BaseUri { get; private set; }

	/// <summary>
	/// Gets the root node of the JSON Schema.
	/// </summary>
	/// <remarks>Use this property to access the top-level schema node, which represents the entry point for
	/// traversing or validating the entire schema structure.</remarks>
	public JsonSchemaNode Root { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private JsonSchema(bool value)
	{
		BoolValue = value;
		Root = value ? JsonSchemaNode.True() : JsonSchemaNode.False();
		BaseUri = Root.BaseUri;
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	private JsonSchema(JsonSchemaNode root, BuildOptions buildOptions, bool refIgnoresSiblingKeywords)
	{
		_buildOptions = buildOptions;
		_refIgnoresSiblingKeywords = refIgnoresSiblingKeywords;
		Root = root;
		BaseUri = Root.BaseUri;
	}

	/// <summary>
	/// Loads text from a file and builds a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="fileName">The filename to load, URL-decoded.</param>
	/// <param name="options">(optional) Serializer options.</param>
	/// <param name="baseUri">(optional) The base URI for this schema.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	/// <remarks>The filename needs to not be URL-encoded as <see cref="Uri"/> attempts to encode it.</remarks>
	public static JsonSchema FromFile(string fileName, BuildOptions? options = null, Uri? baseUri = null)
	{
		var text = File.ReadAllText(fileName);
		baseUri ??= new Uri(fileName);

		return FromText(text, options, baseUri);
	}

	/// <summary>
	/// Builds a <see cref="JsonSchema"/> from text.
	/// </summary>
	/// <param name="jsonText">The text to parse.</param>
	/// <param name="buildOptions">(optional) The build options.</param>
	/// <param name="baseUri">(optional) The base URI for this schema.</param>
	/// <param name="jsonOptions">(optional) Options for parsing a <see cref="JsonDocument"/>.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	public static JsonSchema FromText(string jsonText, BuildOptions? buildOptions = null, Uri? baseUri = null, JsonDocumentOptions? jsonOptions = null)
	{
		var element = JsonDocument.Parse(jsonText, jsonOptions ?? default).RootElement;
		return Build(element, buildOptions, baseUri);
	}

	private static Uri GenerateBaseUri() => new($"https://json-everything.lib/{Guid.NewGuid():N}");

	/// <summary>
	/// Builds a JsonSchema instance from the specified JSON schema root element, applying the provided build options and
	/// base URI if specified.
	/// </summary>
	/// <remarks>The returned schema is registered with the provided or global schema registry. Reference
	/// resolution and cycle detection are performed during the build process. The base URI is set on the resulting schema
	/// and used for reference resolution.</remarks>
	/// <param name="root">The root JsonElement representing the JSON schema to be parsed and built. Must be a valid JSON schema object.</param>
	/// <param name="options">Optional build options that control schema parsing behavior, such as registry usage and dialect settings. If null,
	/// default options are used.</param>
	/// <param name="baseUri">An optional base URI to associate with the schema for resolving references if the schema does not contain an $id
	/// keyword. If null, a default base URI is generated.</param>
	/// <returns>A JsonSchema instance representing the parsed schema. Returns a singleton schema for boolean schemas (<see
	/// langword="true"/> or <see langword="false"/>), or a constructed schema for object-based definitions.</returns>
	public static JsonSchema Build(JsonElement root, BuildOptions? options = null, Uri? baseUri = null)
	{
		options ??= BuildOptions.Default;
		var context = new BuildContext(options, baseUri ?? GenerateBaseUri())
		{
			LocalSchema = root
		};

		var node = BuildNode(context);

		if (node.Source.ValueKind == JsonValueKind.True) return True;
		if (node.Source.ValueKind == JsonValueKind.False) return False;

		var schema = new JsonSchema(node, context.Options, context.Dialect.RefIgnoresSiblingKeywords)
		{
			BaseUri = node.BaseUri
		};
		context.Options.SchemaRegistry.Register(schema);
		context.BaseUri = node.BaseUri;

		schema._resolved = TryResolveReferences(node, context);
		DetectCycles(node);

		return schema;
	}

	/// <summary>
	/// Builds a new JSON schema node from the specified build context, interpreting the local schema and its keywords
	/// according to the active dialect.
	/// </summary>
	/// <remarks>This method processes schema keywords and handles dialect-specific behaviors, such as anchor
	/// registration and embedded resource management. The resulting node reflects the schema's structure and metadata as
	/// defined in the context.
	/// 
	/// Use this method from keywords to build subschemas.
	///
	/// Individual keywords may throw various exceptions during the validation phase.
	/// </remarks>
	/// <param name="context">The build context containing the local schema, dialect, base URI, and options used to construct the schema node.
	/// Must not be null.</param>
	/// <returns>A JsonSchemaNode representing the parsed schema, including its keywords and any registered anchors or embedded
	/// resources.</returns>
	/// <exception cref="ArgumentException">Thrown if the local schema is not a boolean or an object.</exception>
	public static JsonSchemaNode BuildNode(BuildContext context)
	{
		if (context.LocalSchema.ValueKind == JsonValueKind.True)
		{
			var trueNode = JsonSchemaNode.True();
			trueNode.RelativePath = context.RelativePath;
			return trueNode;
		}

		if (context.LocalSchema.ValueKind == JsonValueKind.False)
		{
			var falseNode = JsonSchemaNode.False();
			falseNode.RelativePath = context.RelativePath;
			return falseNode;
		}

		if (context.LocalSchema.ValueKind != JsonValueKind.Object)
			throw new ArgumentException($"Schemas may only booleans or objects.  Received {context.LocalSchema.ValueKind}");

		var onlyHandleRef = context.LocalSchema.TryGetProperty("$ref", out _) &&
		                    context.Dialect.RefIgnoresSiblingKeywords;

		var keywordData = new List<KeywordData>();
		foreach (var property in context.LocalSchema.EnumerateObject())
		{
			var keyword = property.Name;
			var value = property.Value;

			var handler = context.Dialect.GetHandler(keyword);

			var data = new KeywordData(context)
			{
				EvaluationOrder = context.Dialect.GetEvaluationOrder(keyword) ?? 0,
				RawValue = value.Clone(),
				Handler = handler,
				Value = handler is AnnotationKeyword
					? keyword
					: handler.ValidateKeywordValue(value)
			};
			if (handler is SchemaKeyword)
			{
				// can't set the dialect from within the keyword because context is a struct
				var uri = (Uri)data.Value!;
				context.Dialect = context.Options.DialectRegistry.Get(uri, context.Options.SchemaRegistry, context.Options.VocabularyRegistry, context.Dialect);
			}
			else if (handler is IdKeyword && !onlyHandleRef)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				context.PathFromResourceRoot = JsonPointer.Empty;
				context.BaseUri = context.BaseUri.Resolve((Uri)data.Value!);
			}

			var keywordContext = context with
			{
				PathFromResourceRoot = context.PathFromResourceRoot.Combine(context.RelativePath).Combine(keyword)
			};
			handler.BuildSubschemas(data, keywordContext);

			foreach (var subschema in data.Subschemas)
			{
				subschema.PathFromResourceRoot = keywordContext.PathFromResourceRoot.Combine(subschema.RelativePath);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			keywordData.Add(data);
		}

		var embeddedResources = keywordData
			.SelectMany(x => x.Subschemas)
			.Where(x => x.BaseUri != context.BaseUri);
		foreach (var embeddedResource in embeddedResources)
		{
			if (embeddedResource.BaseUri == True.BaseUri || embeddedResource.BaseUri == False.BaseUri) continue;

			var schema = new JsonSchema(embeddedResource, context.Options, context.Dialect.RefIgnoresSiblingKeywords)
			{
				BaseUri = embeddedResource.BaseUri
			};
			context.Options.SchemaRegistry.Register(schema);
		}

		var node = new JsonSchemaNode
		{
			BaseUri = context.BaseUri,
			Source = context.LocalSchema,
			Keywords = keywordData.OrderBy(x => x.EvaluationOrder).ToArray(),
			RelativePath = context.RelativePath
		};

		var oldIdKeyword = keywordData.FirstOrDefault(x => x.Handler is Keywords.Draft06.IdKeyword);
		if (oldIdKeyword is not null)
		{
			var handler = (Keywords.Draft06.IdKeyword)oldIdKeyword.Handler;
			var uri = (Uri)oldIdKeyword.Value!;
			var anchor = uri.OriginalString[1..];
			if (uri.OriginalString.StartsWith("#") && handler.AnchorPattern.IsMatch(anchor))
				context.Options.SchemaRegistry.RegisterAnchor(context.BaseUri, anchor, node);
		}

		var anchorKeyword = keywordData.FirstOrDefault(x => x.Handler is AnchorKeyword);
		if (anchorKeyword is not null)
			context.Options.SchemaRegistry.RegisterAnchor(context.BaseUri, (string)anchorKeyword.Value!, node);

		var dynamicAnchorKeyword = keywordData.FirstOrDefault(x => x.Handler is DynamicAnchorKeyword);
		if (dynamicAnchorKeyword is not null)
			context.Options.SchemaRegistry.RegisterDynamicAnchor(context.BaseUri, (string)dynamicAnchorKeyword.Value!, node);

		var recursiveAnchorKeyword = keywordData.FirstOrDefault(x => x.Handler is Keywords.Draft201909.RecursiveAnchorKeyword);
		if (recursiveAnchorKeyword?.Value is true)
			context.Options.SchemaRegistry.RegisterRecursiveAnchor(context.BaseUri, node);

		return node;
	}

	private static bool TryResolveReferences(JsonSchemaNode node, BuildContext context, HashSet<JsonSchemaNode>? checkedNodes = null)
	{
		var resolved = true;
		checkedNodes ??= [];
		if (!checkedNodes.Add(node)) return resolved;

		var refKeyword = node.Keywords.SingleOrDefault(x => x.Handler is RefKeyword);
		if (refKeyword is not null)
		{
			var handler = (RefKeyword)refKeyword.Handler;
			resolved &= handler.TryResolve(refKeyword, context);
		}

		var dynamicRefKeyword = node.Keywords.SingleOrDefault(x => x.Handler is DynamicRefKeyword);
		if (dynamicRefKeyword is not null)
		{
			var handler = (DynamicRefKeyword)dynamicRefKeyword.Handler;
			resolved &= handler.TryResolve(dynamicRefKeyword, context);
		}

		foreach (var keyword in node.Keywords)
		{
			foreach (var subNode in keyword.Subschemas)
			{
				var subschemaContext = context with
				{
					BaseUri = subNode.BaseUri
				};
				resolved &= TryResolveReferences(subNode, subschemaContext, checkedNodes);
			}
		}

		return resolved;
	}

	private static void DetectCycles(JsonSchemaNode node, HashSet<JsonSchemaNode>? checkedNodes = null)
	{
		checkedNodes ??= [];
		if (!checkedNodes.Add(node)) return;

		var foundRefs = new HashSet<JsonSchemaNode> { node };

		var refKeyword = node.Keywords.SingleOrDefault(x => x.Handler is RefKeyword);
		if (refKeyword is null) return;

		while (refKeyword is not null)
		{
			var nextNode = refKeyword.Subschemas.FirstOrDefault();
			if (nextNode is null) break; // might not have resolved all refs yet

			if (!foundRefs.Add(nextNode))
				throw new JsonSchemaException($"Cycle detected starting with a reference to '{refKeyword.RawValue}'");

			refKeyword = nextNode.Keywords.SingleOrDefault(x => x.Handler is RefKeyword);
		}

		foreach (var subschema in node.Keywords.SelectMany(x => x.Subschemas))
		{
			DetectCycles(subschema, checkedNodes);
		}
	}

	/// <summary>
	/// Evaluates the specified JSON instance against the schema and returns the results.
	/// </summary>
	/// <param name="instance">The JSON data to be evaluated against the schema.</param>
	/// <param name="options">(Optional) Evaluation settings that control validation behavior. If null, default options are used.</param>
	/// <returns>An EvaluationResults object containing the outcome of the schema validation, including any errors or annotations.</returns>
	public EvaluationResults Evaluate(JsonElement instance, EvaluationOptions? options = null)
	{
		if (!_resolved)
		{
			var buildContext = new BuildContext(_buildOptions, BaseUri)
			{
				LocalSchema = Root.Source
			};
			_resolved = TryResolveReferences(Root, buildContext);
		}
		// Let the normal RefResolutionException happen through evaluation if not resolved.

		options ??= EvaluationOptions.Default;
		var context = new EvaluationContext
		{
			Options = options,
			SchemaRegistry = _buildOptions.SchemaRegistry,
			RefIgnoresSiblingKeywords = _refIgnoresSiblingKeywords,
			InstanceRoot = instance,
			Instance = instance,
			EvaluationPath = JsonPointer.Empty,
			Scope = new(BaseUri),
			CanOptimize = options.OutputFormat == OutputFormat.Flag
		};

		var results = Root.Evaluate(context);
		switch (options.OutputFormat)
		{
			case OutputFormat.Flag:
				results.ToFlag();
				break;
			case OutputFormat.List:
				results.ToList();
				break;
			case OutputFormat.Hierarchical:
				break;
			default:
				throw new ArgumentOutOfRangeException("options.OutputFormat");
		}

		return results;
	}

	/// <summary>
	/// Finds the subschema node within the root schema that corresponds to the specified JSON pointer.
	/// </summary>
	/// <param name="pointer">The JSON pointer indicating the location of the subschema to find within the root schema.</param>
	/// <param name="context">The build context used for schema evaluation and node construction if the subschema is not found directly.</param>
	/// <returns>A <see cref="JsonSchemaNode"/> representing the subschema at the specified pointer, or <see langword="null"/> if no
	/// matching subschema exists.</returns>
	public JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildContext context)
	{
		var subschema = _discoveredSchemas?.GetValueOrDefault(pointer);
		if (subschema is not null) return subschema;

		subschema = Root;
		var currentPointer = pointer;
		while (currentPointer.SegmentCount != 0)
		{
			var keyword = subschema.Keywords.FirstOrDefault(x => currentPointer.StartsWith(JsonPointer.Create(x.Handler.Name)));
			if (keyword is null)
			{
				subschema = null;
				break;
			}

			currentPointer = currentPointer.GetLocal(currentPointer.SegmentCount - 1);

			subschema = keyword.Subschemas.FirstOrDefault(x => currentPointer.StartsWith(x.RelativePath));
			if (subschema is null) break;

			currentPointer = currentPointer.GetLocal(currentPointer.SegmentCount - subschema.RelativePath.SegmentCount);
		}

		if (subschema == null)
		{
			var localSchema = pointer.Evaluate(Root.Source);
			if (localSchema.HasValue)
			{
				var newContext = context with
				{
					BaseUri = BaseUri,
					LocalSchema = localSchema.Value,
#pragma warning disable CS0618 // Type or member is obsolete
					PathFromResourceRoot = pointer
				};
				subschema = BuildNode(newContext);
				subschema.PathFromResourceRoot = pointer;
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		if (subschema is not null)
		{
			_discoveredSchemas ??= [];
			_discoveredSchemas[pointer] = subschema;
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

		var idKeyword = Root.Keywords.SingleOrDefault(x => x.Handler is IdKeyword);
		return idKeyword?.RawValue.GetString() ?? BaseUri.OriginalString;
	}
}

/// <summary>
/// Provides custom serialization and deserialization logic for <see cref="JsonSchema"/> objects when using
/// System.Text.Json.
/// </summary>
/// <remarks>Use this converter to enable reading and writing of <see cref="JsonSchema"/> instances with
/// System.Text.Json. This converter handles the mapping between JSON schema representations and <see
/// cref="JsonSchema"/> objects, ensuring correct parsing and formatting during serialization operations.</remarks>
public class JsonSchemaJsonConverter : JsonConverter<JsonSchema>
{
	/// <summary>
	/// Deserializes a JSON value from the specified reader into a <see cref="JsonSchema"/> instance.
	/// </summary>
	/// <param name="reader">The <see cref="Utf8JsonReader"/> positioned at the JSON value to read. The reader must be at the start of a valid
	/// JSON schema object.</param>
	/// <param name="typeToConvert">The type to convert the JSON value to. This parameter is ignored; the method always returns a <see
	/// cref="JsonSchema"/>.</param>
	/// <param name="options">The serializer options to use when reading the JSON value. Must not be <see langword="null"/>.</param>
	/// <returns>A <see cref="JsonSchema"/> instance representing the deserialized schema, or <see langword="null"/> if the JSON
	/// value is not a valid schema.</returns>
	/// <exception cref="JsonException">Thrown when the JSON value cannot be deserialized into a valid <see cref="JsonSchema"/>. See the inner exception
	/// for details.</exception>
	public override JsonSchema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var element = options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonElement);
		try
		{
			return JsonSchema.Build(element);
		}
		catch (Exception e)
		{
			throw new JsonException("An error occurred while deserializing a schema.  See inner exception for details.", e);
		}
	}

	/// <summary>
	/// Writes the specified JSON schema to the provided writer using the given serializer options.
	/// </summary>
	/// <param name="writer">The writer to which the JSON schema will be written. Must not be null.</param>
	/// <param name="value">The JSON schema to write. Must not be null.</param>
	/// <param name="options">The serializer options to use when writing the JSON schema. Must not be null.</param>
	public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
	{
		options.Write(writer, value.Root.Source, JsonSchemaSerializerContext.Default.JsonElement);
	}
}