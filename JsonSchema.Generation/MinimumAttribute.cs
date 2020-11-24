using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinimumAttribute : Attribute
	{
		public uint Value { get; }

		public MinimumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MinimumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MinimumIntent(attribute.Value));
		}
	}
}