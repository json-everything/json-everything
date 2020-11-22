using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	public static class JsonSchemaBuilderExtensions
	{
		private static readonly List<ISchemaGenerator> _generators =
			typeof(ISchemaGenerator).Assembly
				.DefinedTypes
				.Where(t => typeof(ISchemaGenerator).IsAssignableFrom(t) &&
				            !t.IsInterface &&
				            !t.IsAbstract)
				.Select(Activator.CreateInstance)
				.Cast<ISchemaGenerator>()
				.ToList();

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
