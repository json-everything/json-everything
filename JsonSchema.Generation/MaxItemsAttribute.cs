using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxItemsAttribute : Attribute
	{
		public uint Value { get; }

		public MaxItemsAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MaxItemsAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MaxItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsArray()) return;

			builder.MaxItems(attribute.Value);
		}
	}
}