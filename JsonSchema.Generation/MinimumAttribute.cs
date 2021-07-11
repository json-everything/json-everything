using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `minimum` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MinimumAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// The minimum.
		/// </summary>
		public decimal Value { get; }

		/// <summary>
		/// Creates a new <see cref="MinimumAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public MinimumAttribute(double value)
		{
			Value = Convert.ToDecimal(value);
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MinimumIntent(attribute.Value));
		}
	}
}