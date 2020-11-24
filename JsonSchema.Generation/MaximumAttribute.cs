using System;
using System.Linq;
using Json.Schema.Generation.Intents;

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
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MaximumIntent(attribute.Value));
		}
	}
}