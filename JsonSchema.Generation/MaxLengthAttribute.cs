using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `maxLength` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxLengthAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// The maximum length.
		/// </summary>
		public uint Length { get; }

		/// <summary>
		/// Creates a new <see cref="MaxLengthAttribute"/> instance.
		/// </summary>
		/// <param name="length">The value.</param>
		public MaxLengthAttribute(uint length)
		{
			Length = length;
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MaxLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			context.Intents.Add(new MaxLengthIntent(attribute.Length));
		}
	}
}