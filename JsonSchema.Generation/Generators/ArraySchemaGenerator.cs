using Json.Schema.Generation.Intents;
using System;
using System.Linq;

namespace Json.Schema.Generation.Generators
{
	internal class ArraySchemaGenerator : BaseReferenceTypeGenerator
	{
		protected override SchemaValueType Type { get; } = SchemaValueType.Array;

		public override bool Handles(Type type)
		{
			return type.IsArray();
		}

		public override void AddConstraints(SchemaGeneratorContext context)
		{
			base.AddConstraints(context);

			Type? itemType = null;

			if (context.Type.IsGenericType)
				itemType = context.Type.GetGenericArguments().First();
			else if (context.Type.IsArray)
				itemType = context.Type.GetElementType();

			if (itemType == null) return;
			var itemContext = SchemaGenerationContextCache.Get(itemType, context.Attributes, context.Configuration);

			context.Intents.Add(new ItemsIntent(itemContext));
		}
	}
}