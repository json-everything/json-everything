using System;
using System.Collections.Generic;

namespace Json.Schema;

/// <summary>
/// A registry for schemas.
/// </summary>
public class SchemaRegistry
{
	internal class Registration
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
		_registered ??= new Dictionary<Uri, Registration>();
		uri = MakeAbsolute(uri);
		var registration = CheckRegistry(_registered, uri);
		if (registration == null)
			_registered[uri] = new Registration{Root = document};
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
