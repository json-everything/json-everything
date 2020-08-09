using System;
using System.Collections.Generic;

namespace Json.Schema
{
	public class SchemaRegistry
	{
		private Dictionary<Uri, JsonSchema> _registered;

		public static SchemaRegistry Global { get; } = new SchemaRegistry();

		public void Register(Uri uri, JsonSchema schema)
		{
			_registered ??= new Dictionary<Uri, JsonSchema>();
			throw new NotImplementedException();
		}

		public JsonSchema Get(Uri uri)
		{
			// check local
			if (_registered == null)
			{
				
			}
			// if not found, check global
			if (!ReferenceEquals(Global, this))
			{

			}
			throw new NotImplementedException();
		}
	}
}