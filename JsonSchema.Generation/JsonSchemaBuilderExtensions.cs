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
			var context = new SchemaGeneratorContext(type, new List<Attribute>());

			context.GenerateIntents();

			context.Optimize();

			context.Apply(builder);

			return builder;
		}
	}
}
