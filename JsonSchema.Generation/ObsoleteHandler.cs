using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	internal class ObsoleteHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<ObsoleteAttribute>().FirstOrDefault();
			if (attribute == null) return;

			builder.Deprecated(true);
		}
	}
}