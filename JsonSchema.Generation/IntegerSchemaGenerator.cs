using System;

namespace Json.Schema.Generation
{
	internal class IntegerSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(byte) ||
			       type == typeof(short) ||
				   type == typeof(ushort) ||
				   type == typeof(int) ||
				   type == typeof(uint) ||
				   type == typeof(long) ||
				   type == typeof(ulong);
		}

		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			builder.Type(SchemaValueType.Integer);
		}
	}
}