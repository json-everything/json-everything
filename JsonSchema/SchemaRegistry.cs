using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// A registry for schemas.
/// </summary>
public class SchemaRegistry
{
	private class Anchor
	{
#pragma warning disable 8618
		public JsonSchema Schema { get; set; }
#pragma warning restore 8618
		public bool HasDynamic { get; set; }
		public int DynamicSequence { get; set; } = int.MaxValue;
	}

	private class Registration
	{
		private Dictionary<string, Anchor>? _anchors;

		public JsonSchema Root { get; set; } = null!;

		public Dictionary<string, Anchor> Anchors => _anchors ??= new Dictionary<string, Anchor>();
	}

	private static readonly Uri _empty = new("http://everything.json/");

	private Dictionary<Uri, Registration>? _registered;
	private Func<Uri, JsonSchema?>? _fetch;
	private Stack<Uri>? _scopes;

	/// <summary>
	/// The global registry.
	/// </summary>
	public static SchemaRegistry Global { get; } = new();

	/// <summary>
	/// Gets or sets a method to enable automatic download of schemas by `$id` URI.
	/// </summary>
	public Func<Uri, JsonSchema?> Fetch
	{
		get => _fetch ??= _ => null;
		set => _fetch = value;
	}

	static SchemaRegistry()
	{
		Global.InitializeMetaSchemas();
	}

	internal SchemaRegistry()
	{
	}

	internal void InitializeMetaSchemas()
	{
		Register(MetaSchemas.Draft6Id, MetaSchemas.Draft6);
		MetaSchemas.Draft6.RecompileAs(Draft.Draft6);

		Register(MetaSchemas.Draft7Id, MetaSchemas.Draft7);
		MetaSchemas.Draft7.RecompileAs(Draft.Draft7);

		Register(MetaSchemas.Draft201909Id, MetaSchemas.Draft201909);
		Register(MetaSchemas.Core201909Id, MetaSchemas.Core201909);
		Register(MetaSchemas.Applicator201909Id, MetaSchemas.Applicator201909);
		Register(MetaSchemas.Validation201909Id, MetaSchemas.Validation201909);
		Register(MetaSchemas.Metadata201909Id, MetaSchemas.Metadata201909);
		Register(MetaSchemas.Format201909Id, MetaSchemas.Format201909);
		Register(MetaSchemas.Content201909Id, MetaSchemas.Content201909);
		MetaSchemas.Draft201909.RecompileAs(Draft.Draft201909);

		Register(MetaSchemas.Draft202012Id, MetaSchemas.Draft202012);
		Register(MetaSchemas.Core202012Id, MetaSchemas.Core202012);
		Register(MetaSchemas.Applicator202012Id, MetaSchemas.Applicator202012);
		Register(MetaSchemas.Validation202012Id, MetaSchemas.Validation202012);
		Register(MetaSchemas.Metadata202012Id, MetaSchemas.Metadata202012);
		Register(MetaSchemas.Unevaluated202012Id, MetaSchemas.Unevaluated202012);
		Register(MetaSchemas.FormatAnnotation202012Id, MetaSchemas.FormatAnnotation202012);
		Register(MetaSchemas.FormatAssertion202012Id, MetaSchemas.FormatAssertion202012);
		Register(MetaSchemas.Content202012Id, MetaSchemas.Content202012);
		MetaSchemas.Draft202012.RecompileAs(Draft.Draft202012);

		Register(MetaSchemas.DraftNextId, MetaSchemas.DraftNext);
		Register(MetaSchemas.CoreNextId, MetaSchemas.CoreNext);
		Register(MetaSchemas.ApplicatorNextId, MetaSchemas.ApplicatorNext);
		Register(MetaSchemas.ValidationNextId, MetaSchemas.ValidationNext);
		Register(MetaSchemas.MetadataNextId, MetaSchemas.MetadataNext);
		Register(MetaSchemas.UnevaluatedNextId, MetaSchemas.UnevaluatedNext);
		Register(MetaSchemas.FormatAnnotationNextId, MetaSchemas.FormatAnnotationNext);
		Register(MetaSchemas.FormatAssertionNextId, MetaSchemas.FormatAssertionNext);
		Register(MetaSchemas.ContentNextId, MetaSchemas.ContentNext);
		MetaSchemas.DraftNext.RecompileAs(Draft.DraftNext);
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="uri">The URI ID of the schema..</param>
	/// <param name="schema">The schema.</param>
	public void Register(Uri? uri, JsonSchema schema)
	{
		RegisterSchema(uri, schema);
	}

	internal void RegisterSchema(Uri? uri, JsonSchema schema)
	{
		_registered ??= new Dictionary<Uri, Registration>();
		uri = MakeAbsolute(uri);
		var registration = CheckRegistry(_registered, uri);
		if (registration == null)
			_registered[uri] = registration = new Registration();
		registration.Root = schema;
	}

	/// <summary>
	/// Registers a schema by a named anchor.
	/// </summary>
	/// <param name="uri">The URI ID of the schema.</param>
	/// <param name="anchor">The anchor name.</param>
	/// <param name="schema">The schema.</param>
	public void RegisterAnchor(Uri? uri, string anchor, JsonSchema schema)
	{
		_registered ??= new Dictionary<Uri, Registration>();
		uri = MakeAbsolute(uri);
		var registration = CheckRegistry(_registered, uri);
		if (registration == null)
			_registered[uri] = registration = new Registration();
		if (!registration.Anchors.ContainsKey(anchor))
			registration.Anchors[anchor] = new Anchor { Schema = schema };
	}

	internal void RegisterDynamicAnchor(Uri uri, string anchor, JsonSchema schema)
	{
		_registered ??= new Dictionary<Uri, Registration>();
		var registration = CheckRegistry(_registered, uri);
		if (registration == null)
			_registered[uri] = registration = new Registration();
		if (registration.Anchors.TryGetValue(anchor, out var existing))
			existing.HasDynamic = true;
		else
			registration.Anchors[anchor] = new Anchor { Schema = schema, HasDynamic = true };
	}

	internal bool DynamicScopeDefinesAnchor(Uri uri, string anchor)
	{
		_registered ??= new Dictionary<Uri, Registration>();
		var registration = CheckRegistry(_registered, uri);
		if (registration == null && !ReferenceEquals(Global, this))
			registration = CheckRegistry(Global._registered!, uri);
		if (registration == null) return false;
		if (!registration.Anchors.TryGetValue(anchor, out var existing)) return false;

		return existing.HasDynamic;
	}

	/// <summary>
	/// Gets a schema by URI ID and/or anchor.
	/// </summary>
	/// <param name="uri">The URI ID.</param>
	/// <param name="anchor">(optional) The anchor name.</param>
	/// <returns>
	/// The schema, if registered in either this or the global registry;4
	/// otherwise null.
	/// </returns>
	// For URI equality see https://docs.microsoft.com/en-us/dotnet/api/system.uri.op_equality?view=netcore-3.1
	// tl;dr - URI equality doesn't consider fragments
	public JsonSchema? Get(Uri? uri, string? anchor = null)
	{
		var registration = GetRegistration(uri);

		if (registration == null) return null;

		if (string.IsNullOrEmpty(anchor)) return registration.Root;
		return registration.Anchors.TryGetValue(anchor!, out var registeredAnchor) ? registeredAnchor.Schema : null;
	}

	internal void Clear()
	{
		_registered?.Clear();
	}

	internal JsonSchema? GetDynamic(Uri? uri, string? anchor)
	{
		var firstAnchor = _registered?.SelectMany(x => x.Value.Anchors)
			.Where(x => x.Key == anchor && x.Value.HasDynamic)
			.Select(x => x.Value)
			.OrderBy(x => x.DynamicSequence)
			.FirstOrDefault();
		if (firstAnchor != null) return firstAnchor.Schema;

		Registration? registration = null;
		uri = MakeAbsolute(uri);

		// check local
		if (_registered != null)
			registration = CheckRegistry(_registered, uri);
		// if not found, check global
		if (registration == null && !ReferenceEquals(Global, this))
			registration = CheckRegistry(Global._registered!, uri);

		if (_scopes != null && registration != null && !string.IsNullOrEmpty(anchor) &&
			registration.Anchors.TryGetValue(anchor!, out var anchorRegistration) &&
			anchorRegistration.HasDynamic)
		{
			// Stacks iterate their values in Pop order.  Since we want the one at the root, we reverse.
			foreach (var scope in _scopes.Reverse())
			{
				registration = GetRegistration(scope);
				registration!.Anchors.TryGetValue(anchor!, out var registeredAnchor);
				if (registeredAnchor?.HasDynamic == true)
					return registeredAnchor.Schema;
			}
		}

		return Get(uri, anchor);
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

	internal void EnteringUriScope(Uri uri)
	{
		Registration? registration = null;
		uri = MakeAbsolute(uri);
		// check local
		if (_registered != null)
			registration = CheckRegistry(_registered, uri);
		// if not found, check global
		if (registration == null && !ReferenceEquals(Global, this))
			registration = CheckRegistry(Global._registered!, uri);

		if (registration != null)
		{
			_scopes ??= new Stack<Uri>();
			_scopes.Push(MakeAbsolute(uri));

			foreach (var anchor in registration.Anchors)
			{
				if (anchor.Value.HasDynamic)
					anchor.Value.DynamicSequence = _registered!.SelectMany(x =>
						x.Value.Anchors.Where(y => y.Key == anchor.Key &&
												   y.Value.HasDynamic &&
												   y.Value.DynamicSequence != int.MaxValue)).Count();
			}
		}
	}

	internal void ExitingUriScope()
	{
		var uri = _scopes?.Pop();

		Registration? registration = null;
		uri = MakeAbsolute(uri);
		// check local
		if (_registered != null)
			registration = CheckRegistry(_registered, uri);
		// if not found, check global
		if (registration == null && !ReferenceEquals(Global, this))
			registration = CheckRegistry(Global._registered!, uri);

		if (registration != null)
		{
			foreach (var anchor in registration.Anchors)
			{
				if (anchor.Value.HasDynamic)
					anchor.Value.DynamicSequence = int.MaxValue;
			}
		}
	}

	private static Registration? CheckRegistry(Dictionary<Uri, Registration> lookup, Uri uri)
	{
		return lookup.TryGetValue(uri, out var registration) ? registration : null;
	}

	internal static string GetFullReference(Uri? uri, string? fragment)
	{
		var baseUri = MakeAbsolute(uri).OriginalString;

		if (string.IsNullOrEmpty(fragment)) return baseUri;

		return $"{baseUri}#{fragment}";
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