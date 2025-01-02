using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Refiners;

internal class ArrayItemsRefiner : ISchemaRefiner
{
	public bool ShouldRun(SchemaGenerationContextBase context)
	{
		return context is MemberGenerationContext memberContext && memberContext.Attributes.Count != 0 && 
		       context.Type.IsArray();
	}

	public void Run(SchemaGenerationContextBase context)
	{
		var memberContext = (MemberGenerationContext)context;
		// check if any attributes apply to item
		// if not, no adjustment needed
		if (memberContext.Attributes.OfType<INestableAttribute>().All(x => x.GenericParameter == -1)) return;

		var refIntent = context.Intents.OfType<RefIntent>().FirstOrDefault();
		if (refIntent is not null)
		{
			context.Intents.Remove(refIntent);
			context.Intents.AddRange(memberContext.BasedOn.Intents);
			memberContext.BasedOn.References.Remove(memberContext);
		}

		var itemsIntent = memberContext.Intents.OfType<ItemsIntent>().FirstOrDefault();
		if (itemsIntent is null) return;

		var itemsTypeContext = itemsIntent.Context as TypeGenerationContext ??
		                       ((MemberGenerationContext)itemsIntent.Context).BasedOn;

		var index = memberContext.Intents.IndexOf(itemsIntent);
		var itemsMemberContext = new MemberGenerationContext(itemsTypeContext, memberContext.Attributes) { Parameter = 0 };
		memberContext.Intents[index] = new ItemsIntent(itemsMemberContext);

		itemsMemberContext.GenerateIntents();
	}
}