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
	public static SchemaRegistry Global { get; } = new();

	/// <summary>
	/// Gets or sets a method to enable automatic download of schemas by `$id` URI.
	/// </summary>
	public Func<Uri, JsonSchema?> Fetch
	{
		get => _fetch ??= _ => null;
		set => _fetch = value;
	}

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="document">The schema.</param>
	public void Register(JsonSchema document) => Register(document.BaseUri, document);

	/// <summary>
	/// Registers a schema by URI.
	/// </summary>
	/// <param name="uri">The URI ID of the schema..</param>
	/// <param name="document">The schema.</param>
	public void Register(Uri? uri, JsonSchema document) => RegisterSchema(uri, document);

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

	internal void RegisterDynamicAnchor(Uri uri, string anchor, JsonSchemaNode node)
	{
		uri = MakeAbsolute(uri);
		var registration = _registered.GetValueOrDefault(uri);
		if (registration != null)
		{
			registration.DynamicAnchors ??= [];
			registration.DynamicAnchors.Add(anchor, node);
			return;
		}

		_registered[uri] = new Registration
		{
			DynamicAnchors = new() { [anchor] = node }
		};
	}

	internal void RegisterRecursiveAnchor(Uri uri, JsonSchemaNode node)
	{
		uri = MakeAbsolute(uri);
		var registration = _registered.GetValueOrDefault(uri);
		if (registration != null)
		{
			registration.RecursiveAnchor = node;
			return;
		}

		_registered[uri] = new Registration
		{
			RecursiveAnchor = node
		};
	}

	private Registration RegisterSchema(Uri? uri, JsonSchema schema)
	{
		var schemaUri = MakeAbsolute(schema.BaseUri);
		var registration = _registered.GetValueOrDefault(schemaUri);
		if (registration == null)
		{
			_registered[schemaUri] = registration = new Registration { Root = schema };
		}
		else
		{
			if (registration.Root is not null && schema != registration.Root)
				throw new JsonSchemaException("Overwriting registered schemas is not permitted.");
			registration.Root = schema;
		}

		if (uri is not null && uri != schemaUri)
		{
			// also register with custom URI
			registration = _registered.First(x => ReferenceEquals(x.Value.Root, schema)).Value;
			_registered[uri] = registration;
		}

		return registration;
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
	public JsonSchema? Get(Uri uri) => GetRegistration(uri)?.Root;

	internal JsonSchemaNode? Get(Uri baseUri, string? anchor)
	{
		var registration = GetRegistration(baseUri);

		if (registration?.Anchors is null) return null;

		return registration.Anchors!.GetValueOrDefault(anchor);
	}

	// need this to check for local dynamic anchor for 2020-12 (bookend)
	internal JsonSchemaNode? GetDynamic(Uri baseUri, string? anchor)
	{
		var registration = GetRegistration(baseUri);

		if (registration?.DynamicAnchors is null) return null;

		return registration.DynamicAnchors!.GetValueOrDefault(anchor);
	}

	internal JsonSchemaNode? GetDynamic(DynamicScope scope, string anchor) =>
		scope
			.Reverse()
			.Select(GetRegistration)
			.Select(x => x?.DynamicAnchors?.GetValueOrDefault(anchor))
			.FirstOrDefault();

	internal JsonSchemaNode? GetRecursive(DynamicScope scope)
	{
		Registration? target = null;
		foreach (var uri in scope)
		{
			var registration = GetRegistration(uri);
			if (registration?.RecursiveAnchor is null) break;

			target = registration;
		}

		return target?.RecursiveAnchor;
	}

	internal JsonSchemaNode? GetRecursive(Uri uri)
	{
		var registration = GetRegistration(uri);
		return registration?.RecursiveAnchor;
	}

	private Registration? GetRegistration(Uri baseUri)
	{
		var registration = _registered.GetValueOrDefault(baseUri) ??
		               Global._registered.GetValueOrDefault(baseUri);

		if (registration is null)
		{
			var remote = Fetch(baseUri) ?? Global.Fetch(baseUri);
			if (remote is not null) 
				registration = RegisterSchema(baseUri, remote);
		}

		return registration;
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
