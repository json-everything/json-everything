using System;
using System.Linq;

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
		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			builder.MinLength(attribute.Length);
		}
	}
}