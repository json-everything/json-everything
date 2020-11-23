using System;
using System.Linq;

namespace Json.Schema.Generation
{
	internal class ArraySchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type.IsArray();
		}

		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			builder.Type(SchemaValueType.Array);

			Type itemType = null;

			if (context.Type.IsGenericType)
				itemType = context.Type.GetGenericArguments().First();
			else if (context.Type.IsArray)
				itemType = context.Type.GetElementType();

			if (itemType == null) return;

			var itemContext = new SchemaGeneratorContext(itemType, context.Attributes);
			builder.Items(new JsonSchemaBuilder().FromType(itemContext));
		}
	}
}