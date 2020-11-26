using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `uniqueItems` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class UniqueItemsAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// Whether the items should be unique.
		/// </summary>
		public bool Value { get; }

		/// <summary>
		/// Creates a new <see cref="UniqueItemsAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public UniqueItemsAttribute(bool value)
		{
			Value = value;
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<UniqueItemsAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsArray() || context.Type == typeof(string)) return;

			context.Intents.Add(new UniqueItemsIntent(attribute.Value));
		}
	}
}