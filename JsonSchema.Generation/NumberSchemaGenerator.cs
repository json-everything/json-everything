using System;
using System.Collections.Generic;

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

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			builder.Type(SchemaValueType.Number);
			builder.HandleAttributes(attributes, type);
		}
	}
}