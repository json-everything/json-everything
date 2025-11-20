using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// A registry for schemas.
/// </summary>
public class SchemaRegistry
{
	private class Registration
	{
		public required IBaseDocument Root { get; init; }
		public Dictionary<string, JsonSchema>? Anchors { get; set; }
		public Dictionary<string, JsonSchema>? LegacyAnchors { get; set; }
		public Dictionary<string, JsonSchema>? DynamicAnchors { get; set; }
		public JsonSchema? RecursiveAnchor { get; set; }
	}

	private static readonly Uri _empty = new("http://everything.json/");

	private readonly Dictionary<Uri, Registration> _registered = [];
	private Func<Uri, IBaseDocument?>? _fetch;

	/// <summary>
	/// The global registry.
	/// </summary>
	public static SchemaRegistry Global => BuildOptions.Default.SchemaRegistry;

	/// <summary>
	/// Gets or sets a method to enable automatic download of schemas by `$id` URI.
	/// </summary>
	public Func<Uri, IBaseDocument?> Fetch
	{
		get => _fetch ??= _ => null;
		set => _fetch = value;
	}

	public SchemaRegistry()
	{
		//Register(MetaSchemas.Draft6);

		//Register(MetaSchemas.Draft7);

		//Register(MetaSchemas.Draft201909);
		//Register(MetaSchemas.Core201909);
		//Register(MetaSchemas.Applicator201909);
		//Register(MetaSchemas.Validation201909);
		//Register(MetaSchemas.Metadata201909);
		//Register(MetaSchemas.Format201909);
		//Register(MetaSchemas.Content201909);

		//Register(MetaSchemas.Draft202012);
		//Register(MetaSchemas.Core202012);
		//Register(MetaSchemas.Applicator202012);
		//Register(MetaSchemas.Validation202012);
		//Register(MetaSchemas.Metadata202012);
		//Register(MetaSchemas.Unevaluated202012);
		//Register(MetaSchemas.FormatAnnotation202012);
		//Register(MetaSchemas.FormatAssertion202012);
		//Register(MetaSchemas.Content202012);

		//Register(MetaSchemas.DraftNext);
		//Register(MetaSchemas.CoreNext);
		//Register(MetaSchemas.ApplicatorNext);
		//Register(MetaSchemas.ValidationNext);
		//Register(MetaSchemas.MetadataNext);
		//Register(MetaSchemas.UnevaluatedNext);
		//Register(MetaSchemas.FormatAnnotationNext);
		//Register(MetaSchemas.FormatAssertionNext);
		//Register(MetaSchemas.ContentNext);
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
	}

	/// <summary>
	/// Registers a new meta-schema URI.
	/// </summary>
	/// <param name="metaSchemaUri">The meta-schema URI.</param>
	/// <param name="metaSchema"></param>
	/// <remarks>
	/// **WARNING** There be dragons here.  Use only if you know what you're doing.
	/// </remarks>
	public static void RegisterNewSpecVersion(Uri metaSchemaUri, IBaseDocument metaSchema)
	{
		// TODO
	}

	private void RegisterSchema(Uri? uri, IBaseDocument document)
	{
		uri = MakeAbsolute(uri);
		var schema = document as JsonSchema;
		var registration = _registered.GetValueOrDefault(uri);
		if (registration != null)
		{
			if (registration.Root != document && schema is not null)
				Initialize(uri, schema);
			return;
		}

		_registered[uri] = new Registration { Root = document };
		if (schema is null) return;

		// TODO: temp
		_registered[uri] = new Registration
		{
			Root = document
		};

		//var registrations = Scan(uri, schema);
		//foreach (var reg in registrations)
		//{
		//	_registered[reg.Key] = reg.Value;
		//}

		if (_registered.ContainsKey(uri)) return;

		// also register with custom URI
		registration = _registered.First(x => ReferenceEquals(x.Value.Root, schema)).Value;
		_registered[uri] = registration;
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
	public IBaseDocument? Get(Uri uri)
	{
		return Get(uri, null);
	}

	internal IBaseDocument? Get(Uri baseUri, string? anchor, bool allowLegacy = false)
	{
		return Get(baseUri, anchor, false, allowLegacy);
	}

	internal JsonSchema Get(DynamicScope scope, Uri baseUri, string anchor, bool requireLocalAnchor)
	{
		baseUri = new Uri(baseUri.GetLeftPart(UriPartial.Query));
	
		if (requireLocalAnchor)
		{
			var registration = _registered.GetValueOrDefault(baseUri) ??
			                   Global._registered.GetValueOrDefault(baseUri) ??
			                   throw new InvalidOperationException($"Could not find '{baseUri}'. This shouldn't happen.");
			if (registration.DynamicAnchors is null || !registration.DynamicAnchors.ContainsKey(anchor))
			{
				var target = Get(baseUri, anchor, false, false) ?? throw new RefResolutionException(baseUri, anchor);
				return (JsonSchema)target;
			}
		}

		foreach (var uri in scope.Reverse())
		{
			var target = Get(uri, anchor, true, false);
			if (target is not null) return (JsonSchema)target;
		}

		throw new RefResolutionException(scope.LocalScope, anchor, true);
	}

	private IBaseDocument? Get(Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		var document = GetFromRegistry(_registered, baseUri, anchor, isDynamic, allowLegacy) ??
		               GetFromRegistry(Global._registered, baseUri, anchor, isDynamic, allowLegacy);

		if (document is null)
		{
			document = Fetch(baseUri) ?? Global.Fetch(baseUri);
			if (document is not null)
			{
				Register(baseUri, document);

				// Fetch() returns the document but not localized to an anchor.
				// Register() scans the document and adds it locally.
				// Now that it's in the local registry, we need to get the target identified by any anchors.
				document = GetFromRegistry(_registered, baseUri, anchor, isDynamic, allowLegacy);
			}
		}

		return document;
	}

	private static IBaseDocument? GetFromRegistry(Dictionary<Uri, Registration> registry, Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		if (!registry.TryGetValue(baseUri, out var registration)) return null;

		if (anchor is null) return registration.Root;

		var anchorList = isDynamic ? registration.DynamicAnchors : registration.Anchors;

		return anchorList?.GetValueOrDefault(anchor) ??
		       (allowLegacy && registration.LegacyAnchors is not null
			       ? registration.LegacyAnchors.GetValueOrDefault(anchor)
			       : null);
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

		foreach (var registration in other._registered)
		{
			_registered[registration.Key] = registration.Value;
		}
	}

	/// <summary>
	/// Sets base URI and spec version for a schema.  Generally not needed as this happens automatically on registration and evaluation.
	/// </summary>
	/// <param name="baseUri">The base URI for the schema.</param>
	/// <param name="schema">The schema</param>
	public void Initialize(Uri baseUri, JsonSchema schema)
	{
		var registrations = Scan(baseUri, schema);
		foreach (var reg in registrations)
		{
			if (_registered.TryGetValue(reg.Key, out var registration))
			{
				if (reg.Value.LegacyAnchors != null)
				{
					registration.LegacyAnchors ??= [];
					foreach (var anchor in reg.Value.LegacyAnchors)
					{
						registration.LegacyAnchors[anchor.Key] = anchor.Value;
					}
				}

				if (reg.Value.Anchors != null)
				{
					registration.Anchors ??= [];
					foreach (var anchor in reg.Value.Anchors)
					{
						registration.Anchors[anchor.Key] = anchor.Value;
					}
				}

				if (reg.Value.DynamicAnchors is not null)
				{
					registration.DynamicAnchors ??= [];
					foreach (var anchor in reg.Value.DynamicAnchors)
					{
						registration.DynamicAnchors[anchor.Key] = anchor.Value;
					}
				}

				registration.RecursiveAnchor = reg.Value.RecursiveAnchor;
			}
			else
				_registered[reg.Key] = reg.Value;

			//SetDialect((JsonSchema)reg.Value.Root, null);
		}
	}

	private Dictionary<Uri, Registration> Scan(Uri baseUri, JsonSchema document)
	{
		//var toCheck = new Queue<(Uri, JsonSchema, SpecVersion, Vocabulary[]?)>();
		//toCheck.Enqueue((baseUri, document, _options.EvaluateAs, null));

		//var registrations = new Dictionary<Uri, Registration>();

		//while (toCheck.Count != 0)
		//{
		//	var (currentUri, currentSchema, parentSpecVersion, parentDialect) = toCheck.Dequeue();

		//	SetDialect(currentSchema, parentDialect);
			
		//	var id = currentSchema.GetId();
			
		//	DetermineSpecVersion(currentSchema, this);
		//	if (currentSchema.DeclaredVersion is SpecVersion.Unspecified)
		//		currentSchema.DeclaredVersion = parentSpecVersion;
		//	if ((currentSchema.DeclaredVersion is not (SpecVersion.Draft6 or SpecVersion.Draft7) || currentSchema.GetRef() is null) && id is not null)
		//		currentUri = new Uri(currentUri, id);

		//	if (!registrations.TryGetValue(currentUri, out var registration))
		//		registrations[currentUri] = registration = new Registration
		//		{
		//			Root = currentSchema
		//		};

		//	currentSchema.BaseUri = currentUri;

		//	if (!string.IsNullOrEmpty(currentUri.Fragment))
		//	{
		//		var fragment = currentUri.Fragment[1..];
		//		if (AnchorKeyword.AnchorPattern201909.IsMatch(fragment))
		//		{
		//			registration.LegacyAnchors ??= [];
		//			registration.LegacyAnchors[fragment] = currentSchema;
		//		}
		//	}

		//	var dynamicAnchor = currentSchema.GetDynamicAnchor();
		//	if (dynamicAnchor is not null)
		//	{
		//		registration.Anchors ??= [];
		//		registration.Anchors[dynamicAnchor] = currentSchema;
		//		registration.DynamicAnchors ??= [];
		//		registration.DynamicAnchors[dynamicAnchor] = currentSchema;
		//	}

		//	var recursiveAnchor = currentSchema.GetRecursiveAnchor();
		//	if (recursiveAnchor == true)
		//		registration.RecursiveAnchor = currentSchema;

		//	var anchor = currentSchema.GetAnchor();
		//	if (anchor is not null)
		//	{
		//		registration.Anchors ??= [];
		//		registration.Anchors[anchor] = currentSchema;
		//	}

		//	using var owner = MemoryPool<JsonSchema>.Shared.Rent(currentSchema.CountSubschemas());
		//	foreach (var subschema in currentSchema.GetSubschemas(owner))
		//	{
		//		if (subschema.BoolValue.HasValue) continue;

		//		toCheck.Enqueue((currentUri, subschema, currentSchema.DeclaredVersion, currentSchema.Dialect));
		//	}
		//}

		//return registrations;

		throw new NotImplementedException();
	}

	//private static void DetermineSpecVersion(JsonSchema schema, SchemaRegistry registry)
	//{
	//	if (schema.BoolValue.HasValue)
	//	{
	//		schema.DeclaredVersion = SpecVersion.Unspecified;
	//		return;
	//	}

	//	if (schema.TryGetKeyword<SchemaKeyword>(SchemaKeyword.Name, out var schemaKeyword))
	//	{
	//		var metaSchemaId = schemaKeyword.Schema;
	//		while (metaSchemaId != null)
	//		{
	//			if (_versionLookup.TryGetValue(metaSchemaId, out var version))
	//			{
	//				schema.DeclaredVersion = version;
	//				return;
	//			}

	//			schema.DeclaredVersion = SpecVersion.Unspecified;

	//			var metaSchema = registry.Get(metaSchemaId) as JsonSchema ??
	//				throw new JsonSchemaException("Cannot resolve custom meta-schema.");

	//			var newMetaSchemaId = metaSchema.GetSchema();
	//			if (newMetaSchemaId == metaSchemaId)
	//				throw new JsonSchemaException("Custom meta-schema `$schema` keywords must eventually resolve to a meta-schema for a supported specification version.");

	//			metaSchemaId = newMetaSchemaId;
	//		}
	//	}

	//	schema.DeclaredVersion = SpecVersion.Unspecified;
	//}

	//private void SetDialect(JsonSchema schema, Vocabulary[]? parentDialect)
	//{
	//	var schemaKeyword = (SchemaKeyword?)schema.Keywords?.FirstOrDefault(x => x is SchemaKeyword);
	//	if (schemaKeyword == null)
	//	{
	//		schema.Dialect = parentDialect;
	//		return;
	//	}

	//	var metaSchema = (JsonSchema)Get(schemaKeyword.Schema);
	//	var vocabulary = metaSchema.GetVocabulary();


	//	if (vocabulary is null) return;

	//	using var owner = MemoryPool<Vocabulary>.Shared.Rent();
	//	var span = owner.Memory.Span;
	//	var i = 0;

	//	foreach (var kvp in vocabulary)
	//	{
	//		var vocab = VocabularyRegistry.Get(kvp.Key);
	//		if (vocab is null)
	//		{
	//			if (kvp.Value && _options.ValidateAgainstMetaSchema)
	//				throw new JsonSchemaException($"Detected unknown required vocabulary: '{kvp.Key}'");
	//			continue;
	//		}

	//		span[i] = vocab;
	//		i++;
	//	}

	//	schema.Dialect = span[..i].ToArray();
	//}
}
