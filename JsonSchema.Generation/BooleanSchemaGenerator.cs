using System;

namespace Json.Schema.Generation
{
	internal class BooleanSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(bool);
		}

		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			builder.Type(SchemaValueType.Boolean);
		}
	}
}