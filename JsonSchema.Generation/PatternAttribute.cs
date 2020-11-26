using System;
using System.Linq;
using JetBrains.Annotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Applies a `pattern` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PatternAttribute : Attribute, IAttributeHandler
	{
		/// <summary>
		/// The regular expression pattern.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Creates a new <see cref="PatternAttribute"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public PatternAttribute([RegexPattern] string value)
		{
			Value = value;
		}

		void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<PatternAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			context.Intents.Add(new PatternIntent(attribute.Value));
		}
	}
}