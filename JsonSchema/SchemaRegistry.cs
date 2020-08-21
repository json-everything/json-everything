using System;
using System.Collections.Generic;

namespace Json.Schema
{
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

		public static SchemaRegistry Global { get; }

		static SchemaRegistry()
		{
			Global = new SchemaRegistry();

			Global.Register(MetaSchemas.Draft6_Id, MetaSchemas.Draft6);
			Global.Register(MetaSchemas.Draft7_Id, MetaSchemas.Draft7);
			Global.Register(MetaSchemas.Draft2019_09_Id, MetaSchemas.Draft2019_09);
		}

		public void Register(Uri uri, JsonSchema schema)
		{
			_registered ??= new Dictionary<Uri, Registration>();
			uri = MakeAbsolute(uri);
			var registry = CheckRegistry(_registered, uri);
			if (registry == null)
				_registered[uri] = registry = new Registration();
			registry.Root = schema;
		}

		public void RegisterAnchor(Uri uri, string anchor, JsonSchema schema)
		{
			_registered ??= new Dictionary<Uri, Registration>();
			uri = MakeAbsolute(uri);
			var registry = CheckRegistry(_registered, uri);
			if (registry == null)
				_registered[uri] = registry = new Registration();
			registry.Anchors[anchor] = schema;
		}

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
	}
}