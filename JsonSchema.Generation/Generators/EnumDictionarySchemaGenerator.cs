using Json.Schema.Generation.Intents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Json.Schema.Generation.Generators
{
	internal class EnumDictionarySchemaGenerator : BaseReferenceTypeGenerator
	{
		protected override SchemaValueType Type { get; } = SchemaValueType.Object;

		public override bool Handles(Type type)
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

		public override void AddConstraints(SchemaGeneratorContext context)
		{
			base.AddConstraints(context);

			var keyType = context.Type.GenericTypeArguments[0];
			var keyContext = SchemaGenerationContextCache.Get(keyType, context.Attributes, context.Configuration);
			context.Intents.Add(new PropertyNamesIntent(keyContext));

			var valueType = context.Type.GenericTypeArguments[1];
			var valueContext = SchemaGenerationContextCache.Get(valueType, context.Attributes, context.Configuration);

			context.Intents.Add(new AdditionalPropertiesIntent(valueContext));
		}
	}
}