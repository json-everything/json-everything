using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Generators;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Tracks the available generators.
	/// </summary>
	public static class GeneratorRegistry
	{
		private static readonly List<ISchemaGenerator> _generators =
			new List<ISchemaGenerator>
			{
				new NullableValueTypeSchemaGenerator(),
				new BooleanSchemaGenerator(),
				new IntegerSchemaGenerator(),
				new NumberSchemaGenerator(),
				new StringSchemaGenerator(),
				new EnumGenerator(),
				new DateTimeSchemaGenerator(),
				new GuidSchemaGenerator(),
				new JsonPointerSchemaGenerator(),
				new UriSchemaGenerator(),
				// the dictionary ones are enumerable, so they need to come before the array one
				new StringDictionarySchemaGenerator(),
				new EnumDictionarySchemaGenerator(),
				new ArraySchemaGenerator(),
				// this must always be last because it thinks it can do everything
				new ObjectSchemaGenerator()
			};

		/// <summary>
		/// Registers a new generator.
		/// </summary>
		/// <param name="generator">The generator.</param>
		/// <remarks>
		/// Registration is order dependent: last one wins.  If you have multiple generators which
		/// can handle a given type, the last one registered will be used.
		/// </remarks>
		public static void Register(ISchemaGenerator generator)
		{
			_generators.Insert(0, generator);
		}

		internal static ISchemaGenerator? Get(Type type)
		{
			return _generators.FirstOrDefault(g => g.Handles(type));
		}
	}
}