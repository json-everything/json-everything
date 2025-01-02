using System;
using System.Diagnostics.CodeAnalysis;

namespace Json.Schema.Generation;

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
	[RequiresDynamicCode("This method uses reflection to query types and is not suited for AOT scenarios.")]
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
	[RequiresDynamicCode("This method uses reflection to query types and is not suited for AOT scenarios.")]
	public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type, SchemaGeneratorConfiguration? configuration = null)
	{
		SchemaGeneratorConfiguration.Current = configuration ?? new SchemaGeneratorConfiguration();

		SchemaGenerationContextCache.Clear();
		var context = SchemaGenerationContextCache.GetRoot(type);

		//if (SchemaGeneratorConfiguration.Current.Optimize)
		//	SchemaGenerationContextOptimizer.Optimize();

		context.Apply(builder);

		return builder;
	}
}