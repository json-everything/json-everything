using System;
using System.Collections;
using System.Collections.Generic;
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
		else if (typeof(IEnumerable).IsAssignableFrom(context.Type))
			itemType = context.Type.GetInterfaces()
				.FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				?.GetGenericArguments().First();

		if (itemType == null) return;
		var itemTypeContext = SchemaGenerationContextCache.Get(itemType);
		SchemaGenerationContextBase itemContext = itemTypeContext;
		if (context is MemberGenerationContext memberContext && memberContext.Attributes.Count != 0)
		{
			itemContext = new MemberGenerationContext(itemTypeContext, memberContext.Attributes);
		}

		context.Intents.Add(new ItemsIntent(itemContext));
	}
}