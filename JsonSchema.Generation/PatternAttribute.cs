using System;
using System.Linq;
using JetBrains.Annotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PatternAttribute : Attribute
	{
		public string Value { get; }

		public PatternAttribute([RegexPattern] string value)
		{
			Value = value;
		}
	}

	internal class PatternAttributeHandler : IAttributeHandler
	{
		public void AddConstraints(SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<PatternAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			context.Intents.Add(new PatternIntent(attribute.Value));
		}
	}
}