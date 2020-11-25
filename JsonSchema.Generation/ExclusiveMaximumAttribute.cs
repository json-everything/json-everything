using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies an `exclusiveMaximum` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMaximumAttribute : Attribute
	{
		/// <summary>
		/// The exclusive maximum.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="ExclusiveMaximumAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExclusiveMaximumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class ExclusiveMaximumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<ExclusiveMaximumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new ExclusiveMaximumIntent(attribute.Value));
		}
	}
}