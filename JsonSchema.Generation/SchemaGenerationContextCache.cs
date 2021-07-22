using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Gets the contexts for the current run.
	/// </summary>
	public static class SchemaGenerationContextCache
	{
		private class Key
		{
			public Type? Type { get; }
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

			public override bool Equals(object? obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((Key) obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((Type?.GetHashCode() ?? 0) * 397) ^ Hash;
				}
			}
		}

		[ThreadStatic]
		private static Dictionary<Key, SchemaGeneratorContext>? _cache;

		private static Dictionary<Key, SchemaGeneratorContext> Cache => _cache ??= new Dictionary<Key, SchemaGeneratorContext>();

		/// <summary>
		/// Gets or creates a <see cref="SchemaGeneratorContext"/> based on the given
		/// type and attribute set.
		/// </summary>
		/// <param name="type">The type to generate.</param>
		/// <param name="attributes">The attribute set on the property.</param>
		/// <param name="configuration">The generator configuration.</param>
		/// <returns>
		/// A generation context, from the cache if one exists with the specified
		/// type and attribute set; otherwise a new one.  New contexts are automatically
		/// cached.
		/// </returns>
		/// <remarks>
		/// Use this in your generator if it needs to create keywords with subschemas.
		/// </remarks>
		public static SchemaGeneratorContext Get(Type type, List<Attribute>? attributes, SchemaGeneratorConfiguration configuration)
		{
			var hash = attributes?.GetAttributeSetHashCode() ?? 0;
			var key = new Key(type, hash);
			if (!Cache.TryGetValue(key, out var context))
			{
				context = new SchemaGeneratorContext(type, attributes!, configuration);
				Cache[key] = context;
				context.GenerateIntents();
			}

			return context;
		}

		/// <summary>
		/// (Obsolete) Gets or creates a <see cref="SchemaGeneratorContext"/> based on the given
		/// type and attribute set.
		/// </summary>
		/// <param name="type">The type to generate.</param>
		/// <param name="attributes">The attribute set on the property.</param>
		/// <returns>
		/// A generation context, from the cache if one exists with the specified
		/// type and attribute set; otherwise a new one.  New contexts are automatically
		/// cached.
		/// </returns>
		/// <remarks>
		/// Use this in your generator if it needs to create keywords with subschemas.
		/// </remarks>
		[Obsolete("Use the overload with SchemaGeneratorConfiguration instead.")]
		public static SchemaGeneratorContext Get(Type type, List<Attribute>? attributes)
		{
			var hash = attributes?.GetAttributeSetHashCode() ?? 0;
			var key = new Key(type, hash);
			if (!Cache.TryGetValue(key, out var context))
			{
				context = new SchemaGeneratorContext(type, attributes!, new SchemaGeneratorConfiguration());
				Cache[key] = context;
				context.GenerateIntents();
			}

			return context;
		}

		internal static void Clear()
		{
			Cache.Clear();
		}
	}
}