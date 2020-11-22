using System;

namespace Json.Schema.Generation
{
	internal class StringSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(string);
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type)
		{
			builder.Type(SchemaValueType.String);
		}
	}
}