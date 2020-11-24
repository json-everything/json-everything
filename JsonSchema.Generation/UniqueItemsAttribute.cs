using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class UniqueItemsAttribute : Attribute
	{
		public bool Value { get; }

		public UniqueItemsAttribute(bool value)
		{
			Value = value;
		}
	}

	internal class UniqueItemsAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<UniqueItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsArray() || context.Type == typeof(string)) return;

			context.Intents.Add(new UniqueItemsIntent(attribute.Value));
		}
	}
}