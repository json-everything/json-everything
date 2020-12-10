using System;
using System.Linq;
using JetBrains.Annotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[UsedImplicitly]
	internal class ObsoleteAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			if (context.Attributes.OfType<ObsoleteAttribute>().Any())
				context.Intents.Add(new DeprecatedIntent(true));
		}
	}
}