using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxLengthAttribute : Attribute
	{
		public uint Length { get; }

		public MaxLengthAttribute(uint length)
		{
			Length = length;
		}
	}

	internal class MaxLengthAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MaxLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (target != typeof(string)) return;

			builder.MaxLength(attribute.Length);
		}
	}
}