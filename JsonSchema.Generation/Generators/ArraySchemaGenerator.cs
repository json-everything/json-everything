using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators;

internal class ArraySchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type.IsArray();
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		context.Intents.Add(new TypeIntent(SchemaValueType.Array));

		Type? itemType = null;

		if (context.Type.IsGenericType)
			itemType = context.Type.GetGenericArguments().First();
		else if (context.Type.IsArray)
			itemType = context.Type.GetElementType();

		if (itemType == null) return;
		var itemContext = context is MemberGenerationContext memberContext
			? SchemaGenerationContextCache.Get(itemType, memberContext.Attributes)
			: SchemaGenerationContextCache.Get(itemType);

		context.Intents.Add(new ItemsIntent(itemContext));
	}
}