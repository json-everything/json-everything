using System;
using System.Linq;
using Json.Schema.Generation.Intents;

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
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MultipleOfAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MultipleOfIntent(attribute.Value));
		}
	}
}