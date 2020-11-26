using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class NumberSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(float) ||
			       type == typeof(double) ||
			       type == typeof(decimal);
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.Number));
		}
	}
}