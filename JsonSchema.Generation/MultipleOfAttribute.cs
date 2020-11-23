using System;
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
		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MultipleOfAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			builder.MultipleOf(attribute.Value);
		}
	}
}