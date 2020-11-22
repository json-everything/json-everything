using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinItemsAttribute : Attribute
	{
		public uint Value { get; }

		public MinItemsAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MinItemsAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder propertyBuilder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MinItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsArray()) return;

			propertyBuilder.MinItems(attribute.Value);
		}
	}
}