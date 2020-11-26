using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class EnumGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type.IsEnum;
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			var values = Enum.GetNames(context.Type).ToList();

			context.Intents.Add(new EnumIntent(values));
		}
	}
}