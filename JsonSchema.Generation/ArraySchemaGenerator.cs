using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	internal class ArraySchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type.IsArray();
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			builder.Type(SchemaValueType.Array);

			Type itemType = null;

			if (type.IsGenericType)
				itemType = type.GetGenericArguments().First();
			else if (type.IsArray)
				itemType = type.GetElementType();

			if (itemType == null) return;

			builder.Items(new JsonSchemaBuilder().FromType(itemType, attributes));
			builder.HandleAttributes(attributes, type);
		}
	}
}