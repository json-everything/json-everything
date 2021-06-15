using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Provides extension methods for schema generation.
	/// </summary>
	public static class JsonSchemaBuilderExtensions
	{
		/// <summary>
		/// Generates a schema from a CLR type.
		/// </summary>
		/// <typeparam name="T">The type to generate.</typeparam>
		/// <param name="builder">The schema builder.</param>
		/// <param name="configuration">The generator configuration.</param>
		/// <returns>The schema builder (for fluent syntax support).</returns>
		public static JsonSchemaBuilder FromType<T>(this JsonSchemaBuilder builder, SchemaGeneratorConfiguration? configuration = null)
		{
			return FromType(builder, typeof(T), configuration);
		}

		/// <summary>
		/// Generates a schema from a CLR type.
		/// </summary>
		/// <param name="builder">The schema builder.</param>
		/// <param name="type">The type to generate.</param>
		/// <param name="configuration">The generator configuration.</param>
		/// <returns>The schema builder (for fluent syntax support).</returns>
		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type, SchemaGeneratorConfiguration? configuration = null)
		{
			configuration ??= new SchemaGeneratorConfiguration();

			SchemaGenerationContextCache.Clear();
			var context = SchemaGenerationContextCache.Get(type, new List<Attribute>(), configuration);

			context.Optimize();

			context.Apply(builder);

			return builder;
		}
	}
}
