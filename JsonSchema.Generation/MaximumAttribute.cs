using System;
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
		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			builder.Maximum(attribute.Value);
		}
	}
}