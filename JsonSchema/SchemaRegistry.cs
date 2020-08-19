using System;
using System.Collections.Generic;

namespace Json.Schema
{
	public class SchemaRegistry
	{
		private Dictionary<Uri, JsonSchema> _registered;

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
			_registered ??= new Dictionary<Uri, JsonSchema>();
			_registered[uri] = schema;
		}

		// For URI equality see https://docs.microsoft.com/en-us/dotnet/api/system.uri.op_equality?view=netcore-3.1
		// tl;dr - URI equality doesn't consider fragments
		public JsonSchema Get(Uri uri)
		{
			JsonSchema schema = null;
			// check local
			if (_registered != null)
				schema = CheckRegistry(_registered, uri);
			// if not found, check global
			if (schema == null && !ReferenceEquals(Global, this)) 
				schema = CheckRegistry(Global._registered, uri);

			return schema;
		}

		private static JsonSchema CheckRegistry(Dictionary<Uri, JsonSchema> lookup, Uri uri)
		{
			return lookup.TryGetValue(uri, out var schema) ? schema : null;
		}
	}
}