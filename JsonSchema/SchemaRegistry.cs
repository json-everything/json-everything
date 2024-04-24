using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// A registry for schemas.
/// </summary>
public class SchemaRegistry
{
	private class Registration
	{
		public required IBaseDocument Root { get; init; } = null!;
		public Dictionary<string, JsonSchema> Anchors { get; } = [];
		public Dictionary<string, JsonSchema> LegacyAnchors { get; } = [];
		public Dictionary<string, JsonSchema> DynamicAnchors { get; } = [];
		public JsonSchema? RecursiveAnchor { get; set; }
	}

	private static readonly Uri _empty = new("http://everything.json/");

	private readonly Dictionary<Uri, Registration> _registered = [];
	private readonly EvaluationOptions _options;
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

	/// <summary>
	/// Creates a new <see cref="SchemaRegistry"/>.
	/// </summary>
	[Obsolete("There should be no reason to create a schema registry.  This is handled internally.")]
	public SchemaRegistry()
	{
		_options = EvaluationOptions.Default;
	}

	internal SchemaRegistry(EvaluationOptions options)
	{
		_options = options;
	}

	internal void InitializeMetaSchemas()
	{
		Register(MetaSchemas.Draft6);

		Register(MetaSchemas.Draft7);

		Register(MetaSchemas.Draft201909);
		Register(MetaSchemas.Core201909);
		Register(MetaSchemas.Applicator201909);
		Register(MetaSchemas.Validation201909);
		Register(MetaSchemas.Metadata201909);
		Register(MetaSchemas.Format201909);
		Register(MetaSchemas.Content201909);

		Register(MetaSchemas.Draft202012);
		Register(MetaSchemas.Core202012);
		Register(MetaSchemas.Applicator202012);
		Register(MetaSchemas.Validation202012);
		Register(MetaSchemas.Metadata202012);
		Register(MetaSchemas.Unevaluated202012);
		Register(MetaSchemas.FormatAnnotation202012);
		Register(MetaSchemas.FormatAssertion202012);
		Register(MetaSchemas.Content202012);

		Register(MetaSchemas.DraftNext);
		Register(MetaSchemas.CoreNext);
		Register(MetaSchemas.ApplicatorNext);
		Register(MetaSchemas.ValidationNext);
		Register(MetaSchemas.MetadataNext);
		Register(MetaSchemas.UnevaluatedNext);
		Register(MetaSchemas.FormatAnnotationNext);
		Register(MetaSchemas.FormatAssertionNext);
		Register(MetaSchemas.ContentNext);
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
		_options.Changed = true;
	}

	private void RegisterSchema(Uri? uri, IBaseDocument document)
	{
		uri = MakeAbsolute(uri);
		var registration = _registered.GetValueOrDefault(uri);
		if (registration != null) return;
		if (document is not JsonSchema schema)
		{
			_registered[uri] = new Registration { Root = document };
			return;
		}
		
		var registrations = Scan(uri, schema);
		foreach (var reg in registrations)
		{
			_registered[reg.Key] = reg.Value;
		}

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
	public IBaseDocument Get(Uri uri)
	{
		return Get(uri, null);
	}

	internal IBaseDocument Get(Uri baseUri, string? anchor, bool allowLegacy = false)
	{
		return Get(baseUri, anchor, false, allowLegacy) ?? throw new RefResolutionException(baseUri, anchor);
	}

	internal JsonSchema Get(DynamicScope scope, Uri baseUri, string anchor, bool requireLocalAnchor)
	{
		if (requireLocalAnchor)
		{
			var registration = _registered.GetValueOrDefault(baseUri) ?? Global._registered.GetValueOrDefault(baseUri);
			if (registration == null)
				throw new InvalidOperationException($"Could not find '{baseUri}'. This shouldn't happen.");
			if (!registration.DynamicAnchors.ContainsKey(anchor))
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

	internal JsonSchema GetRecursive(DynamicScope scope)
	{
		JsonSchema? resolved = null;
		foreach (var uri in scope)
		{
			var registration = _registered.GetValueOrDefault(uri) ?? Global._registered.GetValueOrDefault(uri);
			if (registration == null)
				throw new InvalidOperationException($"Could not find '{uri}'. This shouldn't happen.");
			if (registration.RecursiveAnchor is null)
				return resolved ?? (JsonSchema) registration.Root;

			resolved = registration.RecursiveAnchor;
		}

		// resolved should always be set
		return resolved ?? throw new NotImplementedException();
	}

	private IBaseDocument? Get(Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		var document = GetFromRegistry(_registered, baseUri, anchor, isDynamic, allowLegacy) ??
		               GetFromRegistry(Global._registered, baseUri, anchor, isDynamic, allowLegacy);

		if (document is null)
		{
			document = Fetch(baseUri) ?? Global.Fetch(baseUri);
			if (document is not null)
				Register(baseUri, document);
		}

		return document;
	}

	private static IBaseDocument? GetFromRegistry(Dictionary<Uri, Registration> registry, Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		if (!registry.TryGetValue(baseUri, out var registration)) return null;

		if (anchor is null) return registration.Root;

		var anchorList = isDynamic ? registration.DynamicAnchors : registration.Anchors;

		return anchorList.GetValueOrDefault(anchor) ?? (allowLegacy ? registration.LegacyAnchors.GetValueOrDefault(anchor) : null);
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
				foreach (var anchor in reg.Value.LegacyAnchors)
				{
					registration.LegacyAnchors[anchor.Key] = anchor.Value;
				}
				foreach (var anchor in reg.Value.Anchors)
				{
					registration.Anchors[anchor.Key] = anchor.Value;
				}
				foreach (var anchor in reg.Value.DynamicAnchors)
				{
					registration.DynamicAnchors[anchor.Key] = anchor.Value;
				}
				registration.RecursiveAnchor = reg.Value.RecursiveAnchor;
			}
			else
				_registered[reg.Key] = reg.Value;
		}
	}

	private Dictionary<Uri, Registration> Scan(Uri baseUri, JsonSchema document)
	{
		var toCheck = new Queue<(Uri, JsonSchema, SpecVersion)>();
		toCheck.Enqueue((baseUri, document, _options.EvaluateAs));

		var registrations = new Dictionary<Uri, Registration>();

		while (toCheck.Count != 0)
		{
			var (currentUri, currentSchema, parentSpecVersion) = toCheck.Dequeue();

			var id = currentSchema.GetId();

			DetermineSpecVersion(currentSchema, this);
			if (currentSchema.DeclaredVersion is SpecVersion.Unspecified)
				currentSchema.DeclaredVersion = parentSpecVersion;
			if ((currentSchema.DeclaredVersion is not (SpecVersion.Draft6 or SpecVersion.Draft7) || currentSchema.GetRef() is null) && id is not null)
				currentUri = new Uri(currentUri, id);

			if (!registrations.TryGetValue(currentUri, out var registration))
				registrations[currentUri] = registration = new Registration
				{
					Root = currentSchema
				};

			currentSchema.BaseUri = currentUri;

			if (!string.IsNullOrEmpty(currentUri.Fragment))
			{
				var fragment = currentUri.Fragment[1..];
				if (AnchorKeyword.AnchorPattern201909.IsMatch(fragment))
					registration.LegacyAnchors[fragment] = currentSchema;
			}

			var dynamicAnchor = currentSchema.GetDynamicAnchor();
			if (dynamicAnchor is not null)
			{
				registration.Anchors[dynamicAnchor] = currentSchema;
				registration.DynamicAnchors[dynamicAnchor] = currentSchema;
			}

			var recursiveAnchor = currentSchema.GetRecursiveAnchor();
			if (recursiveAnchor == true)
				registration.RecursiveAnchor = currentSchema;

			var anchor = currentSchema.GetAnchor();
			if (anchor is not null)
				registration.Anchors[anchor] = currentSchema;

			foreach (var subschema in currentSchema.GetSubschemas())
			{
				if (subschema.BoolValue.HasValue) continue;

				toCheck.Enqueue((currentUri, subschema, currentSchema.DeclaredVersion));
			}
		}

		return registrations;
	}

	private static void DetermineSpecVersion(JsonSchema schema, SchemaRegistry registry)
	{
		if (schema.BoolValue.HasValue)
		{
			schema.DeclaredVersion = SpecVersion.Unspecified;
			return;
		}

		if (schema.TryGetKeyword<SchemaKeyword>(SchemaKeyword.Name, out var schemaKeyword))
		{
			var metaSchemaId = schemaKeyword.Schema;
			while (metaSchemaId != null)
			{
				var version = metaSchemaId.OriginalString switch
				{
					MetaSchemas.Draft6IdValue => SpecVersion.Draft6,
					MetaSchemas.Draft7IdValue => SpecVersion.Draft7,
					MetaSchemas.Draft201909IdValue => SpecVersion.Draft201909,
					MetaSchemas.Draft202012IdValue => SpecVersion.Draft202012,
					MetaSchemas.DraftNextIdValue => SpecVersion.DraftNext,
					_ => SpecVersion.Unspecified
				};
				if (version != SpecVersion.Unspecified)
				{
					schema.DeclaredVersion = version;
					return;
				}

				var metaSchema = registry.Get(metaSchemaId) as JsonSchema ??
					throw new JsonSchemaException("Cannot resolve custom meta-schema.");

				if (metaSchema.TryGetKeyword<SchemaKeyword>(SchemaKeyword.Name, out var newMetaSchemaKeyword) &&
					newMetaSchemaKeyword.Schema == metaSchemaId)
					throw new JsonSchemaException("Custom meta-schema `$schema` keywords must eventually resolve to a meta-schema for a supported specification version.");

				metaSchemaId = newMetaSchemaKeyword?.Schema;
			}
		}

		schema.DeclaredVersion = SpecVersion.Unspecified;
	}

	//private bool TryGetVocab(JsonSchema schema, out Vocabulary[]? vocab)
	//{
	//	var schemaKeyword = (SchemaKeyword?)schema.Keywords?.FirstOrDefault(x => x is SchemaKeyword);
	//	if (schemaKeyword == null)
	//	{
	//		vocab = null;
	//		return false;
	//	}

	//	var metaSchema = (JsonSchema)Get(schemaKeyword.Schema);
	//	var vocabulary = metaSchema.GetVocabulary();

	//	vocab = vocabulary?.Keys.Select(x => VocabularyRegistry.Global.Get(x)!).ToArray();
	//	Dialect[schema.BaseUri] = vocab;
	//	return true;
	//}
}
