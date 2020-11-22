using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class UniqueItemsAttribute : Attribute
	{
		public bool Value { get; }

		public UniqueItemsAttribute(bool value)
		{
			Value = value;
		}
	}

	internal class UniqueItemsAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<UniqueItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsArray()) return;

			builder.UniqueItems(attribute.Value);
		}
	}
}