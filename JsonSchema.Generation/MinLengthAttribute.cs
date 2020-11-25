using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `minimum` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class MinLengthAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// The minimum length.
		/// </summary>
		public uint Length { get; }

		/// <summary>
		/// Creates a new <see cref="MinLengthAttribute"/> instance.
		/// </summary>
		/// <param name="length">The value.</param>
		public MinLengthAttribute(uint length)
		{
			Length = length;
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<MinLengthAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			context.Intents.Add(new MinLengthIntent(attribute.Length));
		}
	}
}