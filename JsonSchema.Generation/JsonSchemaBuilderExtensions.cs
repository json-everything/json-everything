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
		/// <returns>The schema builder (for fluent syntax support).</returns>
		public static JsonSchemaBuilder FromType<T>(this JsonSchemaBuilder builder)
		{
			return FromType(builder, typeof(T));
		}

		/// <summary>
		/// Generates a schema from a CLR type.
		/// </summary>
		/// <param name="builder">The schema builder.</param>
		/// <param name="type">The type to generate.</param>
		/// <returns>The schema builder (for fluent syntax support).</returns>
		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type)
		{
			var context = new SchemaGeneratorContext(type, new List<Attribute>());

			context.GenerateIntents();

			context.Optimize();

			context.Apply(builder);

			return builder;
		}
	}
}
