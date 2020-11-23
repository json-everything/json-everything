using System;

namespace Json.Schema.Generation
{
	internal class NumberSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(float) ||
			       type == typeof(double) ||
			       type == typeof(decimal);
		}

		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			builder.Type(SchemaValueType.Number);
			builder.HandleAttributes(context);
		}
	}
}