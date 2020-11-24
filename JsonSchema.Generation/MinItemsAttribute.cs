using System;
using System.Linq;
using Json.Schema.Generation.Intents;

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
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsArray()) return;

			context.Intents.Add(new MinItemsIntent(attribute.Value));
		}
	}
}