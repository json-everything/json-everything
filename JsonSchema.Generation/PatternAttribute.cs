using System;
using System.Linq;
using JetBrains.Annotations;

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
		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			var attribute = context.Attributes.OfType<PatternAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (context.Type != typeof(string)) return;

			builder.Pattern(attribute.Value);
		}
	}
}