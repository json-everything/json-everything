using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinimumAttribute : Attribute
	{
		public uint Value { get; }

		public MinimumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MinimumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder propertyBuilder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsNumber()) return;

			propertyBuilder.Minimum(attribute.Value);
		}
	}
}