using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class GuidSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(Guid);
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.String));
			context.Intents.Add(new FormatIntent(Formats.Uuid));
		}
	}
}