using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMaximumAttribute : Attribute
	{
		public uint Value { get; }

		public ExclusiveMaximumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class ExclusiveMaximumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<ExclusiveMaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new ExclusiveMaximumIntent(attribute.Value));
		}
	}
}