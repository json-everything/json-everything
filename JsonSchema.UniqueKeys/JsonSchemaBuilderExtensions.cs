using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema.UniqueKeys
{
	/// <summary>
	/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	public static class JsonSchemaBuilderExtensions
	{
		/// <summary>
		/// Adds a `uniqueKeys` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="keys">The collection of pointers to the keys which should be unique within the array.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder UniqueKeys(this JsonSchemaBuilder builder, IEnumerable<JsonPointer> keys)
		{
			builder.Add(new UniqueKeysKeyword(keys));
			return builder;
		}

		/// <summary>
		/// Adds a `uniqueKeys` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="keys">The collection of pointers to the keys which should be unique within the array.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder UniqueKeys(this JsonSchemaBuilder builder, IEnumerable<string> keys)
		{
			builder.Add(new UniqueKeysKeyword(keys.Select(x => JsonPointer.Parse(x))));
			return builder;
		}

		/// <summary>
		/// Adds a `uniqueKeys` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="keys">The collection of pointers to the keys which should be unique within the array.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder UniqueKeys(this JsonSchemaBuilder builder, params JsonPointer[] keys)
		{
			builder.Add(new UniqueKeysKeyword(keys));
			return builder;
		}

		/// <summary>
		/// Adds a `uniqueKeys` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="keys">The collection of pointers to the keys which should be unique within the array.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder UniqueKeys(this JsonSchemaBuilder builder, params string[] keys)
		{
			builder.Add(new UniqueKeysKeyword(keys.Select(x => JsonPointer.Parse(x))));
			return builder;
		}
	}
}