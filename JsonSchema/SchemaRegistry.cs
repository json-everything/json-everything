using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema
{
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
			public bool HasStatic { get; set; }
		}

		private class Registration
		{
			private Dictionary<string, Anchor>? _anchors;

			public JsonSchema Root { get; set; } = null!;

			public Dictionary<string, Anchor> Anchors => _anchors ??= new Dictionary<string, Anchor>();
		}

		private static readonly Uri _empty = new Uri("http://everything.json/");

		private Dictionary<Uri, Registration>? _registered;
		private Func<Uri, JsonSchema?>? _fetch;
		private Stack<Uri>? _scopes;
		private ValidationOptions? _options;

		/// <summary>
		/// The global registry.
		/// </summary>
		public static SchemaRegistry Global { get; }

		/// <summary>
		/// Gets or sets a method to enable automatic download of schemas by `$id` URI.
		/// </summary>
		public Func<Uri, JsonSchema?> Fetch
		{
			get => _fetch ??= _ => null;
			set => _fetch = value;
		}

		internal Draft ValidatingAs => _options!.ValidatingAs;

		static SchemaRegistry()
		{
			Global = new SchemaRegistry();
		}

		internal SchemaRegistry(ValidationOptions options)
		{
			_options = options;
		}
		private SchemaRegistry(){}

		internal static void Initialize()
		{
			if (Global._options != null) return;

			lock (Global)
			{
				if (Global._options != null) return;
				Global._options = ValidationOptions.Default;

				MetaSchemas.Draft6.RegisterSubschemas(Global, MetaSchemas.Draft6Id);

				MetaSchemas.Draft7.RegisterSubschemas(Global, MetaSchemas.Draft7Id);

				MetaSchemas.Draft201909.RegisterSubschemas(Global, MetaSchemas.Draft201909Id);
				MetaSchemas.Core201909.RegisterSubschemas(Global, MetaSchemas.Core201909Id);
				MetaSchemas.Applicator201909.RegisterSubschemas(Global, MetaSchemas.Applicator201909Id);
				MetaSchemas.Validation201909.RegisterSubschemas(Global, MetaSchemas.Validation201909Id);
				MetaSchemas.Metadata201909.RegisterSubschemas(Global, MetaSchemas.Metadata201909Id);
				MetaSchemas.Format201909.RegisterSubschemas(Global, MetaSchemas.Format201909Id);
				MetaSchemas.Content201909.RegisterSubschemas(Global, MetaSchemas.Content201909Id);

				MetaSchemas.Draft202012.RegisterSubschemas(Global, MetaSchemas.Draft202012Id);
				MetaSchemas.Core202012.RegisterSubschemas(Global, MetaSchemas.Core202012Id);
				MetaSchemas.Applicator202012.RegisterSubschemas(Global, MetaSchemas.Applicator202012Id);
				MetaSchemas.Validation202012.RegisterSubschemas(Global, MetaSchemas.Validation202012Id);
				MetaSchemas.Metadata202012.RegisterSubschemas(Global, MetaSchemas.Metadata202012Id);
				MetaSchemas.Unevaluated202012.RegisterSubschemas(Global, MetaSchemas.Unevaluated202012Id);
				MetaSchemas.FormatAnnotation202012.RegisterSubschemas(Global, MetaSchemas.FormatAnnotation202012Id);
				MetaSchemas.FormatAssertion202012.RegisterSubschemas(Global, MetaSchemas.FormatAssertion202012Id);
				MetaSchemas.Content202012.RegisterSubschemas(Global, MetaSchemas.Content202012Id);
			}
		}

		/// <summary>
		/// Registers a schema by URI.
		/// </summary>
		/// <param name="uri">The URI ID of the schema..</param>
		/// <param name="schema">The schema.</param>
		public void Register(Uri? uri, JsonSchema schema)
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
			if (registration.Anchors.TryGetValue(anchor, out var existing))
				existing.HasStatic = true;
			else
				registration.Anchors[anchor] = new Anchor {Schema = schema, HasStatic = true};
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
				registration.Anchors[anchor] = new Anchor {Schema = schema, HasDynamic = true};
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

		internal JsonSchema? GetDynamic(Uri? uri, string? anchor)
		{
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
				schema.RegisterSubschemas(this, uri);
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
			}

		}

		internal void ExitingUriScope()
		{
			_scopes?.Pop();
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
}