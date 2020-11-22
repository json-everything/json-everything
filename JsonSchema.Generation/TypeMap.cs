using System;
using System.Collections.Concurrent;

namespace Json.Schema.Generation
{
	internal static class TypeMap
	{
		private static readonly ConcurrentDictionary<Type, JsonSchemaBuilder> _registry = new ConcurrentDictionary<Type, JsonSchemaBuilder>();

		public static void Add(Type type, JsonSchemaBuilder schema)
		{
			_registry[type] = schema;
		}

		public static JsonSchemaBuilder Get(Type type)
		{
			_registry.TryGetValue(type, out var schema);
			return schema;
		}
	}
}