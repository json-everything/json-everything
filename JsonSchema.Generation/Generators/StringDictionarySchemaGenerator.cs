using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class StringDictionarySchemaGenerator : ISchemaGenerator
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
			return keyType == typeof(string);
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.Object));

			var valueType = context.Type.GenericTypeArguments[1];
			var valueContext = SchemaGenerationContextCache.Get(valueType, context.Attributes, context.Configuration);

			context.Intents.Add(new AdditionalPropertiesIntent(valueContext));
		}
	}
}