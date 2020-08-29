using System;
using System.Collections.Generic;

namespace Json.Schema
{
	/// <summary>
	/// A registry for schemas.
	/// </summary>
	public class SchemaRegistry
	{
		private class Registration
		{
			private Dictionary<string, JsonSchema> _anchors;
			public JsonSchema Root { get; set; }

			public Dictionary<string, JsonSchema> Anchors => _anchors ??= new Dictionary<string, JsonSchema>();
		}

		private static readonly Uri _empty = new Uri("http://everything.json/");

		private Dictionary<Uri, Registration> _registered;

		/// <summary>
		/// The global registry.
		/// </summary>
		public static SchemaRegistry Global { get; }

		static SchemaRegistry()
		{
			Global = new SchemaRegistry();

			Global.Register(MetaSchemas.Draft6Id, MetaSchemas.Draft6);
			Global.Register(MetaSchemas.Draft7Id, MetaSchemas.Draft7);
			Global.Register(MetaSchemas.Draft201909Id, MetaSchemas.Draft201909);
			Global.Register(MetaSchemas.Core201909Id, MetaSchemas.Core201909);
			Global.Register(MetaSchemas.Applicator201909Id, MetaSchemas.Applicator201909);
			Global.Register(MetaSchemas.Validation201909Id, MetaSchemas.Validation201909);
			Global.Register(MetaSchemas.Metadata201909Id, MetaSchemas.Metadata201909);
			Global.Register(MetaSchemas.Format201909Id, MetaSchemas.Format201909);
			Global.Register(MetaSchemas.Content201909Id, MetaSchemas.Content201909);
		}

		internal SchemaRegistry()
		{

		}

		/// <summary>
		/// Registers a schema by URI.
		/// </summary>
		/// <param name="uri">The URI ID of the schema..</param>
		/// <param name="schema">The schema.</param>
		public void Register(Uri uri, JsonSchema schema)
		{
			_registered ??= new Dictionary<Uri, Registration>();
			uri = MakeAbsolute(uri);
			var registry = CheckRegistry(_registered, uri);
			if (registry == null)
				_registered[uri] = registry = new Registration();
			registry.Root = schema;
		}

		/// <summary>
		/// Registers a schema by a named anchor.
		/// </summary>
		/// <param name="uri">The URI ID of the schema.</param>
		/// <param name="anchor">The anchor name.</param>
		/// <param name="schema">The schema.</param>
		public void RegisterAnchor(Uri uri, string anchor, JsonSchema schema)
		{
			_registered ??= new Dictionary<Uri, Registration>();
			uri = MakeAbsolute(uri);
			var registry = CheckRegistry(_registered, uri);
			if (registry == null)
				_registered[uri] = registry = new Registration();
			registry.Anchors[anchor] = schema;
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
		public JsonSchema Get(Uri uri, string anchor = null)
		{
			Registration registration = null;
			uri = MakeAbsolute(uri);
			// check local
			if (_registered != null)
				registration = CheckRegistry(_registered, uri);
			// if not found, check global
			if (registration == null && !ReferenceEquals(Global, this))
				registration = CheckRegistry(Global._registered, uri);

			if (registration == null) return null;
			if (string.IsNullOrEmpty(anchor)) return registration.Root;
			return registration.Anchors.TryGetValue(anchor, out var schema) ? schema : null;
		}

		private static Registration CheckRegistry(Dictionary<Uri, Registration> lookup, Uri uri)
		{
			return lookup.TryGetValue(uri, out var registration) ? registration : null;
		}

		private static Uri MakeAbsolute(Uri uri)
		{
			if (uri == null) return _empty;

			if (uri.IsAbsoluteUri) return uri;

			return new Uri(_empty, uri);
		}

		internal void CopyFrom(SchemaRegistry other)
		{
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