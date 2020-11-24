using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxLengthAttribute : Attribute
	{
		public uint Length { get; }

		public MaxLengthAttribute(uint length)
		{
			Length = length;
		}
	}

	internal class MaxLengthAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MaxLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			context.Intents.Add(new MaxLengthIntent(attribute.Length));
		}
	}
}