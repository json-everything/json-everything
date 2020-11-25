using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class StringSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(string);
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.String));
		}
	}
}