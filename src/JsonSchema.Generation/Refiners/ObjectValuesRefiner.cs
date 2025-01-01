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

		var additionalPropertiesIntent = memberContext.Intents.OfType<AdditionalPropertiesIntent>().FirstOrDefault();
		if (additionalPropertiesIntent is not null)
		{
			var additionalPropertiesTypeContext = additionalPropertiesIntent.Context as TypeGenerationContext ??
			                       ((MemberGenerationContext)additionalPropertiesIntent.Context).BasedOn;

			additionalPropertiesIntent.Context = new MemberGenerationContext(additionalPropertiesTypeContext, memberContext.Attributes);

			AttributeHandler.HandleAttributes(additionalPropertiesTypeContext);
		}

		var unevaluatedPropertiesIntent = memberContext.Intents.OfType<UnevaluatedPropertiesIntent>().FirstOrDefault();
		if (unevaluatedPropertiesIntent is not null)
		{
			var unevaluatedPropertiesTypeContext = unevaluatedPropertiesIntent.Context as TypeGenerationContext ??
			                                       ((MemberGenerationContext?)unevaluatedPropertiesIntent.Context)?.BasedOn;
			if (unevaluatedPropertiesTypeContext is not null)
			{
				unevaluatedPropertiesIntent.Context = new MemberGenerationContext(unevaluatedPropertiesTypeContext, memberContext.Attributes);

				AttributeHandler.HandleAttributes(unevaluatedPropertiesTypeContext);
			}
		}
	}
}