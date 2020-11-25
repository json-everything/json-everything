using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies an `exclusiveMinimum` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMinimumAttribute : Attribute
	{
		/// <summary>
		/// The exclusive minimum.
		/// </summary>
		public uint Value { get; }

		/// <summary>
		/// Creates a new <see cref="ExclusiveMinimumAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExclusiveMinimumAttribute(uint value)
		{
			Value = value;
		}
	}

	internal class ExclusiveMinimumAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<ExclusiveMinimumAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (!context.Type.IsNumber()) return;

			context.Intents.Add(new ExclusiveMinimumIntent(attribute.Value));
		}
	}
}