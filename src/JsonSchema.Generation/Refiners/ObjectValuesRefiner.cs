using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Refiners;

internal class ObjectValuesRefiner : ISchemaRefiner
{
	public bool ShouldRun(SchemaGenerationContextBase context)
	{
		if (!context.Type.IsGenericType) return false;
		if (context is not MemberGenerationContext memberContext ||
		    memberContext.Attributes.Count == 0) return false;
		if (!context.Intents.OfType<AdditionalPropertiesIntent>().Any() &&
		    !context.Intents.OfType<UnevaluatedPropertiesIntent>().Any()) return false;

		var generic = context.Type.GetGenericTypeDefinition();
		return generic == typeof(IDictionary<,>) ||
		       generic == typeof(Dictionary<,>) ||
		       generic == typeof(ConcurrentDictionary<,>);
	}

	public void Run(SchemaGenerationContextBase context)
	{
		var memberContext = (MemberGenerationContext)context;
		var itemsIntent = memberContext.Intents.OfType<ItemsIntent>().First();
		if (itemsIntent.Context is not TypeGenerationContext itemsTypeContext) return;

		itemsIntent.Context = new MemberGenerationContext(itemsTypeContext, memberContext.Attributes);

		AttributeHandler.HandleAttributes(itemsTypeContext);
	}
}