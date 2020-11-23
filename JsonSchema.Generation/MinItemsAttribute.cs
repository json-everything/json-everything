using System;
using System.Linq;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinItemsAttribute : Attribute
	{
		public uint Value { get; }

		public MinItemsAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MinItemsAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsArray()) return;

			builder.MinItems(attribute.Value);
		}
	}
}