using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Data
{
	/// <summary>
	/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	public static class JsonSchemaBuilderExtensions
	{
		/// <summary>
		/// Adds a `data` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="data">The collection of keywords and references.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, Uri> data)
		{
			builder.Add(new DataKeyword(data));
			return builder;
		}

		/// <summary>
		/// Adds a `data` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="data">The collection of keywords and references.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, string> data)
		{
			builder.Add(new DataKeyword(data.ToDictionary(x => x.Key, x => new Uri(x.Value, UriKind.RelativeOrAbsolute))));
			return builder;
		}

		/// <summary>
		/// Adds a `data` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="data">The collection of keywords and references.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, params (string name, Uri reference)[] data)
		{
			builder.Add(new DataKeyword(data.ToDictionary(x => x.name, x => x.reference)));
			return builder;
		}

		/// <summary>
		/// Adds a `data` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="data">The collection of keywords and references.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, params (string name, string reference)[] data)
		{
			builder.Add(new DataKeyword(data.ToDictionary(x => x.name, x => new Uri(x.reference, UriKind.RelativeOrAbsolute))));
			return builder;
		}
	}
}