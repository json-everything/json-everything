using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `minimum` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MinimumAttribute : Attribute
	{
		/// <summary>
		/// The minimum.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="MinimumAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public MinimumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class MinimumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new MinimumIntent(attribute.Value));
		}
	}
}