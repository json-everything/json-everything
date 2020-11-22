using System;
using System.Reflection;

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
		public void AddConstraints(JsonSchemaBuilder objectBuilder, JsonSchemaBuilder propertyBuilder, PropertyInfo property)
		{
			var attribute = property.GetCustomAttribute<MaxLengthAttribute>();
			if (attribute == null) return;

			propertyBuilder.MaxLength(attribute.Length);
		}
	}
}