using System;
using System.Linq;
using Json.More;

namespace Json.Schema.Generation
{
	internal class EnumGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type.IsEnum;
		}

		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var values = Enum.GetNames(context.Type);

			builder.Enum(values.Select(v => v.AsJsonElement()));
		}
	}
}