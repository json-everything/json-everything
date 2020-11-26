using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `multipleOf` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MultipleOfAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// The divisor.
		/// </summary>
		public decimal Value { get; }

		/// <summary>
		/// Creates a new <see cref="MultipleOfAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public MultipleOfAttribute(double value)
		{
			Value = (decimal) value;
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MultipleOfAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MultipleOfIntent(attribute.Value));
		}
	}
}