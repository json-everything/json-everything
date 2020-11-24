using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinLengthAttribute : Attribute
	{
		public uint Length { get; }

		public MinLengthAttribute(uint length)
		{
			Length = length;
		}
	}

	internal class MinLengthAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			context.Intents.Add(new MinLengthIntent(attribute.Length));
		}
	}
}