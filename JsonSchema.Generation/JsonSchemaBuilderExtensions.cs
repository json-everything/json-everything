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
				new ArraySchemaGenerator(),
				new StringDictionarySchemaGenerator(),
				new EnumDictionarySchemaGenerator(),
				// this must always be last because it thinks it can do everything
				new ObjectSchemaGenerator()
			};

		public static JsonSchemaBuilder FromType<T>(this JsonSchemaBuilder builder)
		{
			return FromType(builder, typeof(T));
		}

		public static JsonSchemaBuilder FromType(this JsonSchemaBuilder builder, Type type)
		{
			var generator = _generators.FirstOrDefault(g => g.Handles(type));
			generator?.AddConstraints(builder, type);

			return builder;
		}
	}
}
