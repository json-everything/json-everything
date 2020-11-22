using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	internal class StringSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(string);
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			builder.Type(SchemaValueType.String);
			builder.HandleAttributes(attributes, type);
		}
	}
}