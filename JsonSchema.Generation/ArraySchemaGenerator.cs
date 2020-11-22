using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	internal class ArraySchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			if (type.IsArray || type == typeof(Array)) return true;
			if (!type.IsGenericType) return false;

			var generic = type.GetGenericTypeDefinition();
			return generic == typeof(IEnumerable<>) ||
			       generic == typeof(List<>) ||
			       generic == typeof(Stack<>) ||
			       generic == typeof(Queue<>);
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