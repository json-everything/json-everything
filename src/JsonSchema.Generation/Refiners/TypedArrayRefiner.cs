using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Refiners;

internal class ArrayItemsRefiner : ISchemaRefiner
{
	public bool ShouldRun(SchemaGenerationContextBase context)
	{
		return context is MemberGenerationContext memberContext && memberContext.Attributes.Count != 0 && 
		       context.Type.IsArray() && context.Intents.OfType<ItemsIntent>().Any();
	}

	public void Run(SchemaGenerationContextBase context)
	{
		var memberContext = (MemberGenerationContext)context;
		var itemsIntent = memberContext.Intents.OfType<ItemsIntent>().First();
		if (itemsIntent.Context is not TypeGenerationContext itemsTypeContext) return;

		var index = memberContext.Intents.IndexOf(itemsIntent);
		var itemsMemberContext = new MemberGenerationContext(itemsTypeContext, memberContext.Attributes);
		memberContext.Intents[index] = new ItemsIntent(itemsMemberContext);

		AttributeHandler.HandleAttributes(itemsMemberContext);
	}
}