using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
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

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.Integer));
		}
	}
}
