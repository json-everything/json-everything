using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	internal class EnumDictionarySchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			if (!type.IsGenericType) return false;

			var generic = type.GetGenericTypeDefinition();
			if (generic != typeof(IDictionary<,>) &&
			    generic != typeof(Dictionary<,>) &&
			    generic != typeof(ConcurrentDictionary<,>))
				return false;

			var keyType = type.GenericTypeArguments[0];
			return keyType.IsEnum;
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			builder.Type(SchemaValueType.Object);

			var valueType = type.GenericTypeArguments[1];
			builder.AdditionalProperties(new JsonSchemaBuilder().FromType(valueType, attributes));

			builder.HandleAttributes(attributes, type);
		}
	}
}