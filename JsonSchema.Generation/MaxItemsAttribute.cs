using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxItemsAttribute : Attribute
	{
		public uint Value { get; }

		public MaxItemsAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MaxItemsAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MaxItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsArray()) return;

			context.Intents.Add(new MaxItemsIntent(attribute.Value));
		}
	}
}