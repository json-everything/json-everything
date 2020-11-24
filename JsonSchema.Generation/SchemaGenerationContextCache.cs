using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public static class SchemaGenerationContextCache
	{
		private class Key
		{
			public Type Type { get; }
			public int Hash { get; }

			public Key(Type type, int hash)
			{
				Type = type;
				Hash = hash;
			}

			private bool Equals(Key other)
			{
				return Type == other.Type && Hash == other.Hash;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((Key) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ Hash;
				}
			}
		}

		[ThreadStatic]
		private static Dictionary<Key, SchemaGeneratorContext> _cache;

		private static Dictionary<Key, SchemaGeneratorContext> Cache => _cache ??= new Dictionary<Key, SchemaGeneratorContext>();

		public static SchemaGeneratorContext Get(Type type, List<Attribute> attributes)
		{
			var hash = attributes?.GetTypeBasedHashCode() ?? 0;
			var key = new Key(type, hash);
			if (!Cache.TryGetValue(key, out var context))
			{
				context = new SchemaGeneratorContext(type, attributes);
				_cache[key] = context;
				context.GenerateIntents();
			}

			return context;
		}
	}
}