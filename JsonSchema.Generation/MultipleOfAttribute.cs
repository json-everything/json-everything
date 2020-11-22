using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MultipleOfAttribute : Attribute
	{
		public decimal Value { get; }

		public MultipleOfAttribute(double value)
		{
			Value = (decimal) value;
		}
	}

	internal class MultipleOfAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MultipleOfAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsNumber()) return;

			builder.MultipleOf(attribute.Value);
		}
	}
}