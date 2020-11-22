using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinLengthAttribute : Attribute
	{
		public uint Length { get; }

		public MinLengthAttribute(uint length)
		{
			Length = length;
		}
	}

	internal class MinLengthAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder propertyBuilder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<MinLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (target != typeof(string)) return;

			propertyBuilder.MinLength(attribute.Length);
		}
	}
}