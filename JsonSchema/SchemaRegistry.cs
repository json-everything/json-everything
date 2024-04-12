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
		public IBaseDocument Root { get; set; } = null!;
		public Dictionary<string, JsonSchema> Anchors { get; } = [];
		public Dictionary<string, JsonSchema> LegacyAnchors { get; } = [];
		public Dictionary<string, JsonSchema> DynamicAnchors { get; } = [];
		public JsonSchema? RecursiveAnchor { get; set; }
	}

	private static readonly Uri _empty = new("http://everything.json/");

	private Dictionary<Uri, Registration> _registered = [];
	private Func<Uri, IBaseDocument?>? _fetch;
	private readonly EvaluationOptions? _options;

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
	}
	
	internal SchemaRegistry(EvaluationOptions options)
	{
		_options = options;
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="document">The schema.</param>
	/// <param name="initialVersion">The schema version to start analysis.  Overridden by the presence of a `$schema` keyword.  Default is unspecified.</param>
	public void Register(IBaseDocument document, SpecVersion initialVersion = SpecVersion.Unspecified)
	{
		Register(document.BaseUri, document, initialVersion);
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="uri">The URI ID of the schema.</param>
	/// <param name="document">The schema.</param>
	/// <param name="initialVersion">The schema version to start analysis.  Overridden by the presence of a `$schema` keyword.  Default is unspecified.</param>
	public void Register(Uri? uri, IBaseDocument document, SpecVersion initialVersion = SpecVersion.Unspecified)
	{
		RegisterSchema(uri, document, initialVersion);

		if (_options != null)
			_options.Changed = true;
	}

	internal void SelfRegister(IBaseDocument document, SpecVersion initialVersion)
	{
		RegisterSchema(document.BaseUri, document, initialVersion);
	}

	private void RegisterSchema(Uri? uri, IBaseDocument document, SpecVersion initialVersion = SpecVersion.Unspecified)
	{
		uri = MakeAbsolute(uri);
		var registration = CheckRegistry(_registered, uri);
		if (registration is null)
		{
			var registrations = Scan(uri, document, initialVersion);
			foreach (var subschema in registrations)
			{
				_registered[subschema.Key] = subschema.Value;
			}

			if (!_registered.ContainsKey(uri))
			{
				var found = _registered.FirstOrDefault(x => ReferenceEquals(x.Value.Root, document)).Value ??
				            new Registration { Root = document };
				_registered[uri] = found;
			}
		}
	}

	private Registration? GetRegistration(Uri? uri)
	{
		uri = MakeAbsolute(uri);
		// check local
		var registration = CheckRegistry(_registered, uri);
		// if not found, check global
		if (registration == null && !ReferenceEquals(Global, this))
			registration = CheckRegistry(Global._registered, uri);

		if (registration == null)
		{
			var schema = Fetch(uri) ?? Global.Fetch(uri);
			if (schema == null) return null;

			Register(uri, schema);
			registration = CheckRegistry(_registered, uri);
		}

		return registration;
	}

	private static Registration? CheckRegistry(Dictionary<Uri, Registration> lookup, Uri uri)
	{
		return lookup.GetValueOrDefault(uri);
	}

	private static Uri MakeAbsolute(Uri? uri)
	{
		if (uri == null) return GenerateBaseUri();

		if (uri.IsAbsoluteUri) return uri;

		return new Uri(_empty, uri);
	}

	internal static Uri GenerateBaseUri() => new($"{_empty}{Guid.NewGuid().ToString("N")[..10]}");

	internal void CopyFrom(SchemaRegistry other)
	{
		_fetch = other._fetch;

		if (other._registered.Count == 0) return;

		if (_registered.Count == 0)
		{
			_registered = new Dictionary<Uri, Registration>(other._registered);
			return;
		}

		foreach (var registration in other._registered)
		{
			_registered[registration.Key] = registration.Value;
		}
	}

	/// <summary>
	/// Gets a schema by URI ID and/or anchor.
	/// </summary>
	/// <param name="baseUri">The URI ID.</param>
	/// <param name="anchor">An anchor, if applicable.</param>
	/// <param name="allowLegacy">Specifies whether `$id` is allowed to contain anchors (drafts 6/7 only).</param>
	/// <returns>
	/// The schema, if registered in either this or the global registry; otherwise null.
	/// </returns>
	// For URI equality see https://docs.microsoft.com/en-us/dotnet/api/system.uri.op_equality?view=netcore-3.1
	// tl;dr - URI equality doesn't consider fragments
	public IBaseDocument Get(Uri baseUri, string? anchor = null, bool allowLegacy = false)
	{
		return GetAnchor(baseUri, anchor, false, allowLegacy) ?? throw new SchemaRefResolutionException(baseUri, anchor);
	}

	internal JsonSchema Get(DynamicScope scope, Uri baseUri, string anchor, bool requireLocalAnchor)
	{
		if (requireLocalAnchor)
		{
			var registration = GetRegistration(baseUri);
			if (registration == null)
				throw new InvalidOperationException($"Could not find '{baseUri}'. This shouldn't happen.");
			if (!registration.DynamicAnchors.ContainsKey(anchor))
			{
				var target = GetAnchor(baseUri, anchor, false, false) as JsonSchema ?? throw new SchemaRefResolutionException(baseUri, anchor);
				return target;
			}
		}

		foreach (var uri in scope.Reverse())
		{
			if (GetAnchor(uri, anchor, true, false) is JsonSchema target) return target;
		}

		throw new SchemaRefResolutionException(scope.LocalScope, anchor, true);
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
				return resolved ?? (JsonSchema)registration.Root;

			resolved = registration.RecursiveAnchor;
		}

		// resolved should always be set
		return resolved ?? throw new NotImplementedException();
	}

	private IBaseDocument? GetAnchor(Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		return GetAnchorFromRegistry(baseUri, anchor, isDynamic, allowLegacy);
	}

	private IBaseDocument? GetAnchorFromRegistry(Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		var registration = GetRegistration(baseUri);
		if (registration is null) return null;

		if (anchor is null) return registration.Root;

		var anchorList = isDynamic ? registration.DynamicAnchors : registration.Anchors;

		return anchorList.GetValueOrDefault(anchor) ?? (allowLegacy ? registration.LegacyAnchors.GetValueOrDefault(anchor) : null);
	}

	/// <summary>
	/// Forces a scan of a document.  For schemas, this sets base URIs and spec versions.
	/// </summary>
	/// <param name="baseUri">A preferred base URI for the document.  May be different than what the document declares.</param>
	/// <param name="baseDocument">The document to scan.</param>
	/// <param name="initialVersion">The JSON Schema version to start analysis.  Overridden by the presence of a `$schema` keyword.  Default is unspecified.</param>
	/// <remarks>
	/// It should not be necessary to call this except for when implementing your own <see cref="IBaseDocument"/>.
	/// </remarks>
	public void ForceInitialize(Uri baseUri, IBaseDocument baseDocument, SpecVersion initialVersion = SpecVersion.Unspecified)
	{
		_ = Scan(baseUri, baseDocument, initialVersion);
	}

	private Dictionary<Uri, Registration> Scan(Uri baseUri, IBaseDocument baseDocument, SpecVersion initialVersion)
	{
		var toCheck = new Queue<(Uri, JsonSchema, SpecVersion)>();
		if (baseDocument is JsonSchema schema)
			toCheck.Enqueue((baseUri, schema, initialVersion));

		var registrations = new Dictionary<Uri, Registration>();

		while (toCheck.Any())
		{
			var (currentUri, currentSchema, currentVersion) = toCheck.Dequeue();

			var id = currentSchema.GetId();
			var metaschema = currentSchema.GetSchema();

			if (metaschema is not null && (id is not null || ReferenceEquals(currentSchema, baseDocument)))
				currentSchema.SpecVersion = currentVersion = GetMetaschemaVersion(currentSchema);

			if (id is not null && (currentSchema.GetRef() is null || currentVersion is not (SpecVersion.Draft6 or SpecVersion.Draft7) || ReferenceEquals(currentSchema, baseDocument)))
				currentUri = new Uri(currentUri, id);

			currentSchema.BaseUri = currentUri;

			if (!registrations.TryGetValue(currentUri, out var registration))
				registrations[currentUri] = registration = new Registration
				{
					Root = currentSchema
				};

			var idText = id?.OriginalString;
			if (!string.IsNullOrEmpty(idText) && idText[0] == '#' && AnchorKeyword.AnchorPattern201909.IsMatch(idText[1..]))
				registration.LegacyAnchors[idText[1..]] = currentSchema;

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

			foreach (var subschema in JsonSchema.GetSubschemas(currentSchema))
			{
				toCheck.Enqueue((currentUri, subschema, currentVersion));
			}
		}

		return registrations;
	}

	private SpecVersion GetMetaschemaVersion(JsonSchema? schema)
	{
		Uri? metaschema = null;
		while (schema is not null)
		{
			metaschema = schema.GetSchema();
			switch (metaschema?.OriginalString)
			{
				case null:
					throw new JsonSchemaException("Custom meta-schema `$schema` keywords must eventually resolve to a meta-schema for a supported specification version");
				case MetaSchemas.Draft6IdValue:
					return SpecVersion.Draft6;
				case MetaSchemas.Draft7IdValue:
					return SpecVersion.Draft7;
				case MetaSchemas.Draft201909IdValue:
					return SpecVersion.Draft201909;
				case MetaSchemas.Draft202012IdValue:
					return SpecVersion.Draft202012;
				case MetaSchemas.DraftNextIdValue:
					return SpecVersion.DraftNext;
			}

			schema = GetRegistration(metaschema)?.Root as JsonSchema;
		}

		throw new JsonSchemaException($"Cannot resolve an ancestor of custom meta-schema '{metaschema}'");
	}
}