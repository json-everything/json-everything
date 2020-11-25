using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies an `maxItems` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxItemsAttribute : Attribute
	{
		/// <summary>
		/// The maximum number of items.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MaxItemsAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
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