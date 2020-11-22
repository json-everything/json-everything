using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	public static class AttributeHandler
	{
		private static readonly List<IAttributeHandler> _attributeHandlers =
			typeof(IAttributeHandler).Assembly.DefinedTypes
				.Where(t => typeof(IAttributeHandler).IsAssignableFrom(t) &&
				            !t.IsInterface && !t.IsAbstract)
				.Select(Activator.CreateInstance)
				.Cast<IAttributeHandler>()
				.ToList();

		public static void HandleAttributes(this JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			foreach (var handler in _attributeHandlers)
			{
				handler.AddConstraints(builder, attributes, target);
			}
		}
	}
}