using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// A registry for schemas.
/// </summary>
public class SchemaRegistry
{
	private class Registration
	{
		public IBaseDocument Root { get; set; } = null!;
	}

	private static readonly Uri _empty = new("http://everything.json/");

	private Dictionary<Uri, Registration>? _registered;
	private Func<Uri, IBaseDocument?>? _fetch;

	/// <summary>
	/// The global registry.
	/// </summary>
	public static SchemaRegistry Global => EvaluationOptions.Default.SchemaRegistry;

	/// <summary>
	/// Gets or sets a method to enable automatic download of schemas by `$id` URI.
	/// </summary>
	public Func<Uri, IBaseDocument?> Fetch
	{
		get => _fetch ??= _ => null;
		set => _fetch = value;
	}

	internal void InitializeMetaSchemas()
	{
		JsonSchema.Initialize(MetaSchemas.Draft6, this);

		JsonSchema.Initialize(MetaSchemas.Draft7, this);

		JsonSchema.Initialize(MetaSchemas.Draft201909, this);
		JsonSchema.Initialize(MetaSchemas.Core201909, this);
		JsonSchema.Initialize(MetaSchemas.Applicator201909, this);
		JsonSchema.Initialize(MetaSchemas.Validation201909, this);
		JsonSchema.Initialize(MetaSchemas.Metadata201909, this);
		JsonSchema.Initialize(MetaSchemas.Format201909, this);
		JsonSchema.Initialize(MetaSchemas.Content201909, this);

		JsonSchema.Initialize(MetaSchemas.Draft202012, this);
		JsonSchema.Initialize(MetaSchemas.Core202012, this);
		JsonSchema.Initialize(MetaSchemas.Applicator202012, this);
		JsonSchema.Initialize(MetaSchemas.Validation202012, this);
		JsonSchema.Initialize(MetaSchemas.Metadata202012, this);
		JsonSchema.Initialize(MetaSchemas.Unevaluated202012, this);
		JsonSchema.Initialize(MetaSchemas.FormatAnnotation202012, this);
		JsonSchema.Initialize(MetaSchemas.FormatAssertion202012, this);
		JsonSchema.Initialize(MetaSchemas.Content202012, this);

		JsonSchema.Initialize(MetaSchemas.DraftNext, this);
		JsonSchema.Initialize(MetaSchemas.CoreNext, this);
		JsonSchema.Initialize(MetaSchemas.ApplicatorNext, this);
		JsonSchema.Initialize(MetaSchemas.ValidationNext, this);
		JsonSchema.Initialize(MetaSchemas.MetadataNext, this);
		JsonSchema.Initialize(MetaSchemas.UnevaluatedNext, this);
		JsonSchema.Initialize(MetaSchemas.FormatAnnotationNext, this);
		JsonSchema.Initialize(MetaSchemas.FormatAssertionNext, this);
		JsonSchema.Initialize(MetaSchemas.ContentNext, this);
	}

	internal static void InitializeMetaSchemasAsNodes()
	{
		Global.RegisterUntyped(MetaSchemas.Draft6Id ,JsonSerializer.SerializeToNode(MetaSchemas.Draft6, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Draft7Id ,JsonSerializer.SerializeToNode(MetaSchemas.Draft7, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Draft201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Draft201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Core201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Core201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Applicator201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Applicator201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Validation201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Validation201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Metadata201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Metadata201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Format201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Format201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Content201909Id ,JsonSerializer.SerializeToNode(MetaSchemas.Content201909, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Draft202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Draft202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Core202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Core202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Applicator202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Applicator202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Validation202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Validation202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Metadata202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Metadata202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Unevaluated202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Unevaluated202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.FormatAnnotation202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.FormatAnnotation202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.FormatAssertion202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.FormatAssertion202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.Content202012Id ,JsonSerializer.SerializeToNode(MetaSchemas.Content202012, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.DraftNextId ,JsonSerializer.SerializeToNode(MetaSchemas.DraftNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.CoreNextId ,JsonSerializer.SerializeToNode(MetaSchemas.CoreNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.ApplicatorNextId ,JsonSerializer.SerializeToNode(MetaSchemas.ApplicatorNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.ValidationNextId ,JsonSerializer.SerializeToNode(MetaSchemas.ValidationNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.MetadataNextId ,JsonSerializer.SerializeToNode(MetaSchemas.MetadataNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.UnevaluatedNextId ,JsonSerializer.SerializeToNode(MetaSchemas.UnevaluatedNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.FormatAnnotationNextId ,JsonSerializer.SerializeToNode(MetaSchemas.FormatAnnotationNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.FormatAssertionNextId ,JsonSerializer.SerializeToNode(MetaSchemas.FormatAssertionNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
		Global.RegisterUntyped(MetaSchemas.ContentNextId ,JsonSerializer.SerializeToNode(MetaSchemas.ContentNext, JsonSchemaSerializerContext.Default.JsonSchema)!);
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="document">The schema.</param>
	public void Register(IBaseDocument document)
	{
		Register(document.BaseUri, document);
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="uri">The URI ID of the schema..</param>
	/// <param name="document">The schema.</param>
	public void Register(Uri? uri, IBaseDocument document)
	{
		RegisterSchema(uri, document);

		if (document is JsonSchema schema)
			JsonSchema.Initialize(schema, this, uri);
	}

	internal void RegisterSchema(Uri? uri, IBaseDocument document)
	{
		_registered ??= [];
		uri = MakeAbsolute(uri);
		var registration = CheckRegistry(_registered, uri);
		if (registration == null)
			_registered[uri] = new Registration{Root = document};
	}

	private class UntypedRegistration
	{
		public JsonNode Root { get; set; }
		public Dictionary<string, JsonNode>? Anchors { get; set; }
		public Dictionary<string, JsonNode>? DynamicAnchors { get; set; }
	}

	private readonly ConcurrentDictionary<string, UntypedRegistration> _untyped = new();

	public void RegisterUntyped(Uri uri, JsonNode value)
	{
		if (!uri.IsAbsoluteUri)
			throw new ArgumentException("uri must be absolute for retrieval");

		var baseUri = uri.GetLeftPart(UriPartial.Query);

		var (anchors, dynamicAnchors) = Scan(uri, value);
		var registration = new UntypedRegistration
		{
			Root = value,
			Anchors = anchors,
			DynamicAnchors = dynamicAnchors
		};

		_untyped[baseUri] = registration;
	}

	public JsonNode? GetUntyped(Uri uri, string? anchor = null, string? dynamicAnchor = null)
	{
		if (!uri.IsAbsoluteUri)
			throw new ArgumentException("uri must be absolute for retrieval");

		var baseUri = uri.GetLeftPart(UriPartial.Query);

		var registration = _untyped.TryGetValue(baseUri, out var value)
			? value
			: Global._untyped.TryGetValue(baseUri, out value)
				? value
				: null;

		if (registration is null) return null;

		if (anchor != null)
			return registration.Anchors?.GetValueOrDefault(anchor);
		
		if (dynamicAnchor != null)
			return registration.DynamicAnchors?.GetValueOrDefault(dynamicAnchor);

		return registration.Root;
	}

	// TODO: this probably belongs in the keyword registry
	private readonly List<(JsonPointer Pointer, bool IsContainer)> _schemaLocations =
	[
		(JsonPointer.Create(AdditionalItemsKeyword.Name), false),
		(JsonPointer.Create(AdditionalPropertiesKeyword.Name), false),
		(JsonPointer.Create(AllOfKeyword.Name), true),
		(JsonPointer.Create(AnyOfKeyword.Name), true),
		(JsonPointer.Create(ContainsKeyword.Name), false),
		(JsonPointer.Create(DefinitionsKeyword.Name), true),
		(JsonPointer.Create(DefsKeyword.Name), true),
		(JsonPointer.Create(DependentSchemasKeyword.Name), true),
		(JsonPointer.Create(ElseKeyword.Name), false),
		(JsonPointer.Create(IfKeyword.Name), false),
		(JsonPointer.Create(ItemsKeyword.Name), false),
		(JsonPointer.Create(ItemsKeyword.Name), true),
		(JsonPointer.Create(NotKeyword.Name), false),
		(JsonPointer.Create(OneOfKeyword.Name), true),
		(JsonPointer.Create(PatternPropertiesKeyword.Name), true),
		(JsonPointer.Create(PropertiesKeyword.Name), true),
		(JsonPointer.Create(PropertyNamesKeyword.Name), false),
		(JsonPointer.Create(ThenKeyword.Name), false),
		(JsonPointer.Create(UnevaluatedItemsKeyword.Name), false),
		(JsonPointer.Create(UnevaluatedPropertiesKeyword.Name), false)
	];

	public void RegisterSchemaLocation(JsonPointer pointer, bool isContainer)
	{
		_schemaLocations.Add((pointer, isContainer));
	}

	private (Dictionary<string, JsonNode>, Dictionary<string, JsonNode>) Scan(Uri baseUri, JsonNode root)
	{
		if (root is not JsonObject objRoot) return default;

		var subschemasToScan = new Queue<JsonObject>();
		subschemasToScan.Enqueue(objRoot);

		var nestedResources = new List<JsonObject>();
		var anchors = new Dictionary<string, JsonNode>();
		var dynamicAnchors = new Dictionary<string, JsonNode>();

		void ScanTarget(JsonNode? target)
		{
			if (target is JsonObject obj)
			{
				if (obj.TryGetValue(IdKeyword.Name, out var idNode, out _) && idNode is JsonValue idValue &&
				    Uri.TryCreate(idValue.GetString(), UriKind.RelativeOrAbsolute, out _))
					nestedResources.Add(obj);
				else
					subschemasToScan.Enqueue(obj);
			}
			//else if (target is not JsonValue value || value.GetBool() is null)
			//	throw new ArgumentException("Schema must be a boolean or object");
		}

		while (subschemasToScan.Any())
		{
			var subschema = subschemasToScan.Dequeue();

			if (subschema.TryGetValue(IdKeyword.Name, out var anchor, out _))
			{
				string? anchorString;
				if (anchor is JsonValue anchorValue && (anchorString = anchorValue.GetString()) is not null &&
				    AnchorKeyword.AnchorPattern.IsMatch(anchorString))
					anchors.Add(anchorString, subschema);
			}

			if (subschema.TryGetValue(AnchorKeyword.Name, out anchor, out _))
			{
				string? anchorString;
				if (anchor is not JsonValue anchorValue || (anchorString = anchorValue.GetString()) is null ||
				    !AnchorKeyword.AnchorPattern.IsMatch(anchorString))
					throw new ArgumentException("$anchor must be a valid URI anchor string");
				anchors.Add(anchorString, subschema);
			}

			if (subschema.TryGetValue(DynamicAnchorKeyword.Name, out anchor, out _))
			{
				string? anchorString;
				if (anchor is not JsonValue anchorValue || (anchorString = anchorValue.GetString()) is null ||
				    !AnchorKeyword.AnchorPattern.IsMatch(anchorString))
					throw new ArgumentException("$anchor must be a valid URI anchor string");
				dynamicAnchors.Add(anchorString, subschema);
			}

			foreach (var (pointer, isContainer) in _schemaLocations)
			{
				if (!pointer.TryEvaluate(subschema, out var target)) continue;

				if (isContainer)
				{
					if (target is JsonObject obj)
					{
						foreach (var kvp in obj)
						{
							ScanTarget(kvp.Value);
						}
					}
					else if (target is JsonArray array)
					{
						foreach (var node in array)
						{
							ScanTarget(node);
						}
					}
				}
				else
				{
					ScanTarget(target);
				}
			}
		}

		foreach (var resource in nestedResources)
		{
			var idNode = resource[IdKeyword.Name];
			string? idString;
			if (idNode is not JsonValue idValue || (idString = idValue.GetString()) is null ||
			    !Uri.TryCreate(idString, UriKind.RelativeOrAbsolute, out var id))
				throw new ArgumentException("$id must be a valid URI string");

			if (!id.IsAbsoluteUri)
				id = new Uri(baseUri, id);

			RegisterUntyped(id, resource);
		}

		return (anchors, dynamicAnchors);
	}

	/// <summary>
	/// Gets a schema by URI ID and/or anchor.
	/// </summary>
	/// <param name="uri">The URI ID.</param>
	/// <returns>
	/// The schema, if registered in either this or the global registry;4
	/// otherwise null.
	/// </returns>
	// For URI equality see https://docs.microsoft.com/en-us/dotnet/api/system.uri.op_equality?view=netcore-3.1
	// tl;dr - URI equality doesn't consider fragments
	public IBaseDocument? Get(Uri? uri)
	{
		var registration = GetRegistration(uri);

		return registration?.Root;
	}

	private Registration? GetRegistration(Uri? uri)
	{
		Registration? registration = null;
		uri = MakeAbsolute(uri);
		// check local
		if (_registered != null)
			registration = CheckRegistry(_registered, uri);
		// if not found, check global
		if (registration == null && !ReferenceEquals(Global, this))
			registration = CheckRegistry(Global._registered!, uri);

		if (registration == null)
		{
			var schema = Fetch(uri) ?? Global.Fetch(uri);
			if (schema == null) return null;

			Register(uri, schema);
			registration = CheckRegistry(_registered!, uri);
		}

		return registration;
	}

	private static Registration? CheckRegistry(Dictionary<Uri, Registration> lookup, Uri uri)
	{
		return lookup.TryGetValue(uri, out var registration) ? registration : null;
	}

	private static Uri MakeAbsolute(Uri? uri)
	{
		if (uri == null) return _empty;

		if (uri.IsAbsoluteUri) return uri;

		return new Uri(_empty, uri);
	}

	internal void CopyFrom(SchemaRegistry other)
	{
		_fetch = other._fetch;

		if (other._registered == null) return;

		if (_registered == null)
		{
			_registered = new Dictionary<Uri, Registration>(other._registered);
			return;
		}

		foreach (var registration in other._registered)
		{
			_registered[registration.Key] = registration.Value;
		}
	}
}
