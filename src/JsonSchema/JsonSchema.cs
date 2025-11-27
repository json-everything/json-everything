using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.Pointer;
using Json.Schema.Keywords;
using Json.Schema.Keywords.Draft201909;
using AnchorKeyword = Json.Schema.Keywords.AnchorKeyword;

// ReSharper disable LocalizableElement

namespace Json.Schema;

/// <summary>
/// Represents a JSON Schema.
/// </summary>
[DebuggerDisplay("{ToDebugString()}")]
public class JsonSchema
{
	private readonly BuildOptions _options;

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
	public Uri BaseUri { get; set; }

	public JsonSchemaNode Root { get; }

	private JsonSchema(bool value)
	{
		BoolValue = value;
		Root = value ? JsonSchemaNode.True() : JsonSchemaNode.False();
		BaseUri = Root.BaseUri;
	}

	private JsonSchema(JsonSchemaNode root, BuildOptions options)
	{
		Root = root;
		_options = options;
		BaseUri = Root.BaseUri;
	}

	/// <summary>
	/// Loads text from a file and deserializes a <see cref="JsonSchema"/>.
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
		var schema = FromText(text, options, baseUri);

		if (schema.BaseUri.Host == "json-everything.lib")
		{
			var path = Path.GetFullPath(fileName);
			// For some reason, full *nix file paths (which start with '/') don't work quite right when
			// being prepended with 'file:///'.  It seems the '////' is interpreted as '//' and the
			// first folder in the path is then interpreted as the host.  To account for this, we
			// need to prepend with 'file://' instead.
			var protocol = path.StartsWith("/") ? "file://" : "file:///";
			schema.BaseUri = new Uri($"{protocol}{path}");
		}
		return schema;
	}

	/// <summary>
	/// Deserializes a <see cref="JsonSchema"/> from text.
	/// </summary>
	/// <param name="jsonText">The text to parse.</param>
	/// <param name="options">Serializer options.</param>
	/// <param name="baseUri">(optional) The base URI for this schema.</param>
	/// <returns>A new <see cref="JsonSchema"/>.</returns>
	/// <exception cref="JsonException">Could not deserialize a portion of the schema.</exception>
	public static JsonSchema FromText(string jsonText, BuildOptions? options = null, Uri? baseUri = null)
	{
		var element = JsonDocument.Parse(jsonText).RootElement;
		return Build(element, options, baseUri);
	}

	private static Uri GenerateBaseUri() => new($"https://json-everything.lib/{Guid.NewGuid():N}");

	public static JsonSchema Build(JsonElement root, BuildOptions? options = null, Uri? baseUri = null)
	{
		var context = new BuildContext(options, root, baseUri ?? GenerateBaseUri())
		{
			LocalSchema = root
		};

		var node = BuildNode(context);

		if (node.Source.ValueKind == JsonValueKind.True) return True;
		if (node.Source.ValueKind == JsonValueKind.False) return False;

		var schema = new JsonSchema(node, context.Options) { BaseUri = node.BaseUri };
		context.Options.SchemaRegistry.Register(schema);
		context.BaseUri = node.BaseUri;

		TryResolveReferences(node, context);
		DetectCycles(node);

		return schema;
	}

	public static JsonSchemaNode BuildNode(BuildContext context)
	{
		if (context.LocalSchema.ValueKind == JsonValueKind.True) return JsonSchemaNode.True();
		if (context.LocalSchema.ValueKind == JsonValueKind.False) return JsonSchemaNode.False();

		if (context.LocalSchema.ValueKind != JsonValueKind.Object)
			throw new ArgumentException($"Schemas may only booleans or objects.  Received {context.LocalSchema.ValueKind}");

		var onlyHandleRef = context.LocalSchema.TryGetProperty("$ref", out _) &&
		                    context.Options.KeywordRegistry.RefIgnoresSiblingKeywords;

		var keywordData = new List<KeywordData>();
		foreach (var property in context.LocalSchema.EnumerateObject())
		{
			var keyword = property.Name;
			var value = property.Value;

			var handler = context.Options.KeywordRegistry.GetHandler(keyword);
			// TODO: for v1, throw exception if not x-*

			var data = new KeywordData
			{
				EvaluationOrder = context.Options.KeywordRegistry.GetEvaluationOrder(keyword) ?? 0,
				RawValue = value.Clone(),
				Handler = handler,
				Value = handler is AnnotationKeyword
					? keyword
					: handler.ValidateKeywordValue(value)
			};
			handler.BuildSubschemas(data, context);

			if (handler is IdKeyword)
			{
				if (!onlyHandleRef)
				{
					var newUri = new Uri(context.BaseUri, (Uri)data.Value!);
					context.BaseUri = newUri;
				}
			}
				
			keywordData.Add(data);
		}

		var embeddedResources = keywordData
			.SelectMany(x => x.Subschemas)
			.Where(x => x.BaseUri != context.BaseUri);
		foreach (var embeddedResource in embeddedResources)
		{
			if (embeddedResource.BaseUri == True.BaseUri || embeddedResource.BaseUri == False.BaseUri) continue;

			var schema = new JsonSchema(embeddedResource, context.Options) { BaseUri = embeddedResource.BaseUri };
			context.Options.SchemaRegistry.Register(schema);
		}

		var node = new JsonSchemaNode
		{
			BaseUri = context.BaseUri,
			Source = context.LocalSchema,
			Keywords = keywordData.OrderBy(x => x.EvaluationOrder).ToArray()
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

		var recursiveAnchorKeyword = keywordData.FirstOrDefault(x => x.Handler is RecursiveAnchorKeyword);
		if (recursiveAnchorKeyword?.Value is true)
			context.Options.SchemaRegistry.RegisterRecursiveAnchor(context.BaseUri, node);

		return node;
	}

	public static void TryResolveReferences(JsonSchemaNode node, BuildContext context, HashSet<JsonSchemaNode>? checkedNodes = null)
	{
		checkedNodes ??= [];
		if (!checkedNodes.Add(node)) return;

		var refKeyword = node.Keywords.SingleOrDefault(x => x.Handler is RefKeyword);
		if (refKeyword is not null)
		{
			var handler = (RefKeyword)refKeyword.Handler;
			handler.TryResolve(refKeyword, context);
		}

		var dynamicRefKeyword = node.Keywords.SingleOrDefault(x => x.Handler is DynamicRefKeyword);
		if (dynamicRefKeyword is not null)
		{
			var handler = (DynamicRefKeyword)dynamicRefKeyword.Handler;
			handler.TryResolve(dynamicRefKeyword, context);
		}

		foreach (var keyword in node.Keywords)
		{
			foreach (var subNode in keyword.Subschemas)
			{
				var subschemaContext = context with
				{
					BaseUri = subNode.BaseUri
				};
				TryResolveReferences(subNode, subschemaContext, checkedNodes);
			}
		}
	}

	public static void DetectCycles(JsonSchemaNode node, HashSet<JsonSchemaNode>? checkedNodes = null)
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

	public EvaluationResults Evaluate(JsonElement instance, EvaluationOptions? options = null)
	{
		options ??= EvaluationOptions.Default;
		var context = new EvaluationContext
		{
			Options = options,
			BuildOptions = _options,
			Instance = instance,
			EvaluationPath = JsonPointer.Empty,
			Scope = new(BaseUri)
		};

		return Root.Evaluate(context);
	}

	public JsonSchemaNode? FindSubschema(JsonPointer pointer, BuildContext context)
	{
		var subschema = Root;
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
					LocalSchema = localSchema.Value
				};
				subschema = BuildNode(newContext);
			}
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