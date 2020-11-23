using System;
using System.Linq;

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
		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			builder.Minimum(attribute.Value);
		}
	}
}