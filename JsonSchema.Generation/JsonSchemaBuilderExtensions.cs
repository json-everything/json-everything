using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public static class JsonSchemaBuilderExtensions
	{
		public static JsonSchemaBuilder FromType<T>(this JsonSchemaBuilder builder)
		{
			return FromType(builder, typeof(T));
		}

		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type)
		{
			return FromType(builder, new SchemaGeneratorContext(type, new List<Attribute>()));
		}

		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var generator = GeneratorRegistry.Get(context.Type);
			generator?.AddConstraints(builder, context);
			builder.HandleAttributes(context);

			return builder;
		}
	}

	public class SchemaGeneratorContext
	{
		public Type Type { get; }
		public List<Attribute> Attributes { get; }

		public SchemaGeneratorContext(Type type, List<Attribute> attributes)
		{
			Type = type;
			Attributes = attributes;
		}
	}
}
