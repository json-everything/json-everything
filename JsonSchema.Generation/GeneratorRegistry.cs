using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	public static class GeneratorRegistry
	{
		private static readonly List<ISchemaGenerator> _generators =
			new List<ISchemaGenerator>
			{
				new BooleanSchemaGenerator(),
				new IntegerSchemaGenerator(),
				new NumberSchemaGenerator(),
				new StringSchemaGenerator(),
				new EnumGenerator(),
				// the dictionary ones are enumerable, so they need to come before the array one
				new StringDictionarySchemaGenerator(),
				new EnumDictionarySchemaGenerator(),
				new ArraySchemaGenerator(),
				// this must always be last because it thinks it can do everything
				new ObjectSchemaGenerator()
			};

		public static void Register(ISchemaGenerator generator)
		{
			_generators.Insert(0, generator);
		}

		internal static ISchemaGenerator Get(Type type)
		{
			return _generators.FirstOrDefault(g => g.Handles(type));
		}
	}
}