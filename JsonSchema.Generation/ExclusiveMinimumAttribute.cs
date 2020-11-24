using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMinimumAttribute : Attribute
	{
		public uint Value { get; }

		public ExclusiveMinimumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class ExclusiveMinimumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<ExclusiveMinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new ExclusiveMinimumIntent(attribute.Value));
		}
	}
}