using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `description` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DescriptionAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// The description.
		/// </summary>
		public string Desription { get; }

		/// <summary>
		/// Creates a new <see cref="DescriptionAttribute"/> instance.
		/// </summary>
		/// <param name="description">The value.</param>
		public DescriptionAttribute(string description)
		{
			Desription = description;
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<DescriptionAttribute>().FirstOrDefault();
			if (attribute == null) return;

			context.Intents.Add(new DescriptionIntent(attribute.Desription));
		}
	}
}