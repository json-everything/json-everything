using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaximumAttribute : Attribute
	{
		public uint Value { get; }

		public MaximumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MaximumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder propertyBuilder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsNumber()) return;

			propertyBuilder.Maximum(attribute.Value);
		}
	}
}