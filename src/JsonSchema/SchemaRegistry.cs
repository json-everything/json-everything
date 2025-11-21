using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// A registry for schemas.
/// </summary>
public class SchemaRegistry
{
	private class Registration
	{
		public JsonSchema? Root { get; set; }
		public Dictionary<string, JsonSchemaNode>? Anchors { get; set; }
		public Dictionary<string, JsonSchemaNode>? DynamicAnchors { get; set; }
		public JsonSchemaNode? RecursiveAnchor { get; set; }
	}

	private static readonly Uri _empty = new("https://json-everything.lib/");

	private readonly Dictionary<Uri, Registration> _registered = [];
	private Func<Uri, JsonSchema?>? _fetch;

	/// <summary>
	/// The global registry.
	/// </summary>
	public static SchemaRegistry Global => new();

	/// <summary>
	/// Gets or sets a method to enable automatic download of schemas by `$id` URI.
	/// </summary>
	public Func<Uri, JsonSchema?> Fetch
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
	public void Register(JsonSchema document)
	{
		Register(document.BaseUri, document);
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="uri">The URI ID of the schema..</param>
	/// <param name="document">The schema.</param>
	public void Register(Uri? uri, JsonSchema document)
	{
		RegisterSchema(uri, document);
	}

	///// <summary>
	///// Registers a new meta-schema URI.
	///// </summary>
	///// <param name="metaSchemaUri">The meta-schema URI.</param>
	///// <param name="metaSchema"></param>
	///// <remarks>
	///// **WARNING** There be dragons here.  Use only if you know what you're doing.
	///// </remarks>
	//public static void RegisterNewSpecVersion(Uri metaSchemaUri, JsonSchema metaSchema)
	//{
	//	// TODO
	//}

	internal void RegisterAnchor(Uri uri, string anchor, JsonSchemaNode node)
	{
		uri = MakeAbsolute(uri);
		var registration = _registered.GetValueOrDefault(uri);
		if (registration != null)
		{
			registration.Anchors ??= [];
			registration.Anchors.Add(anchor, node);
			return;
		}

		_registered[uri] = new Registration
		{
			Anchors = new() { [anchor] = node }
		};
	}

	private void RegisterSchema(Uri? uri, JsonSchema schema)
	{
		uri = MakeAbsolute(uri);
		var registration = _registered.GetValueOrDefault(uri);
		if (registration != null)
		{
			if (registration.Root is not null && schema != registration.Root)
				throw new JsonSchemaException("Overwriting registered schemas is not permitted.");
			registration.Root = schema;
			return;
		}

		_registered[uri] = new Registration { Root = schema };
		if (_registered.ContainsKey(schema.BaseUri)) return;

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
	public JsonSchema? Get(Uri uri)
	{
		var registration = GetRegistration(uri);

		return registration?.Root;
	}

	internal JsonSchemaNode? Get(Uri baseUri, string? anchor)
	{
		var registration = GetRegistration(baseUri);

		return registration?.Anchors?.GetValueOrDefault(anchor);
	}

	internal JsonSchemaNode Get(DynamicScope scope, Uri baseUri, string anchor, bool requireLocalAnchor)
	{
		baseUri = new Uri(baseUri.GetLeftPart(UriPartial.Query));
	
		if (requireLocalAnchor)
		{
			var registration = _registered.GetValueOrDefault(baseUri) ??
			                   Global._registered.GetValueOrDefault(baseUri) ??
			                   throw new UnreachableException($"Could not find '{baseUri}'.");
			return registration?.DynamicAnchors?.GetValueOrDefault(anchor) ??
			       throw new RefResolutionException(baseUri, anchor);
		}

		foreach (var uri in scope.Reverse())
		{
			var registration = GetRegistration(uri);
			if (registration is not null)
				return registration?.DynamicAnchors?.GetValueOrDefault(anchor) ??
				       throw new RefResolutionException(baseUri, anchor);
		}

		throw new RefResolutionException(scope.LocalScope, anchor, true);
	}

	private Registration? GetRegistration(Uri baseUri)
	{
		var document = _registered.GetValueOrDefault(baseUri) ??
		               Global._registered.GetValueOrDefault(baseUri);

		if (document is null)
		{
			//document = Fetch(baseUri) ?? Global.Fetch(baseUri);
			//if (document is not null)
			//{
			//	Register(baseUri, document);

			//	// Fetch() returns the document but not localized to an anchor.
			//	// Register() scans the document and adds it locally.
			//	// Now that it's in the local registry, we need to get the target identified by any anchors.
			//	document = GetFromRegistry(_registered, baseUri);
			//}
		}

		return document;
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
}
