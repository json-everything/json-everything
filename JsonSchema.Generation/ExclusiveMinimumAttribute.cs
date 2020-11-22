using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMinimumAttribute : Attribute
	{
		public uint Value { get; }

		public ExclusiveMinimumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class ExclusiveMinimumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder propertyBuilder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<ExclusiveMinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsNumber()) return;

			propertyBuilder.ExclusiveMinimum(attribute.Value);
		}
	}
}