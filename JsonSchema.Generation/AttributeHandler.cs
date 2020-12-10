using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	internal static class AttributeHandler
	{
		private static readonly List<IAttributeHandler> _handlers =
			typeof(IAttributeHandler)
				.Assembly
				.DefinedTypes
				.Where(t => typeof(IAttributeHandler).IsAssignableFrom(t) &&
				            !typeof(Attribute).IsAssignableFrom(t) &&
				            !t.IsAbstract && !t.IsInterface)
				.Select(Activator.CreateInstance)
				.Cast<IAttributeHandler>()
				.ToList();

		internal static void HandleAttributes(SchemaGeneratorContext context)
		{
			var handlers = _handlers.Concat(context.Attributes.OfType<IAttributeHandler>());

			foreach (var handler in handlers)
			{
				handler.AddConstraints(context);
			}
		}
	}
}