using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMaximumAttribute : Attribute
	{
		public uint Value { get; }

		public ExclusiveMaximumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class ExclusiveMaximumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<ExclusiveMaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!target.IsNumber()) return;

			builder.ExclusiveMaximum(attribute.Value);
		}
	}
}