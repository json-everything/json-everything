using System;
using System.Collections.Generic;
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
		public void AddConstraints(JsonSchemaBuilder builder, IEnumerable<Attribute> attributes, Type target)
		{
			var attribute = attributes.OfType<PatternAttribute>().FirstOrDefault();
			if (attribute == null) return;

			if (target != typeof(string)) return;

			builder.Pattern(attribute.Value);
		}
	}
}