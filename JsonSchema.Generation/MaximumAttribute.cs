using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `maximum` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MaximumAttribute : Attribute
	{
		/// <summary>
		/// The maximum.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MaximumAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public MaximumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MaximumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MaximumIntent(attribute.Value));
		}
	}
}