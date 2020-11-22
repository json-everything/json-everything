using System;
using System.Reflection;

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
		public void AddConstraints(JsonSchemaBuilder objectBuilder, JsonSchemaBuilder propertyBuilder, PropertyInfo property)
		{
			var attribute = property.GetCustomAttribute<MinLengthAttribute>();
			if (attribute == null) return;

			propertyBuilder.MinLength(attribute.Length);
		}
	}
}