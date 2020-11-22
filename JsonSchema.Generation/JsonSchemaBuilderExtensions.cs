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
			return FromType(builder, type, new List<Attribute>());
		}

		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			var generator = GeneratorRegistry.Get(type);
			generator?.AddConstraints(builder, type, attributes);

			return builder;
		}
	}
}
