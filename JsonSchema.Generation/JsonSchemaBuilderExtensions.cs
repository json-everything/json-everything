using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	public static class JsonSchemaBuilderExtensions
	{
		private static readonly List<ISchemaGenerator> _generators =
			new List<ISchemaGenerator>
			{
				new BooleanSchemaGenerator(),
				new IntegerSchemaGenerator(),
				new NumberSchemaGenerator(),
				new StringSchemaGenerator(),
				new ArraySchemaGenerator()
			};

		public static JsonSchemaBuilder FromType<T>(this JsonSchemaBuilder builder)
		{
			return FromType(builder, typeof(T));
		}

		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type)
		{
			var schema = TypeMap.Get(type);
			if (schema != null) return schema;

			foreach (var generator in _generators.Where(g => g.Handles(type)))
			{
				generator.AddConstraints(builder, type);
			}

			return builder;
		}
	}
}
