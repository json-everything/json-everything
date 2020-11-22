using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	internal class BooleanSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(bool);
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			builder.Type(SchemaValueType.Boolean);
			builder.HandleAttributes(attributes, type);
		}
	}
}