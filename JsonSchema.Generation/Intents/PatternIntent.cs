using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.Generation.Intents
{
	internal class PatternIntent : ISchemaKeywordIntent
	{
		public string Value { get; }

		public PatternIntent([RegexPattern] string value)
		{
			Value = value;
		}

		public IEnumerable<SchemaGeneratorContext> GetChildContexts()
		{
			return Enumerable.Empty<SchemaGeneratorContext>();
		}

		public void Replace(int hashCode, SchemaGeneratorContext newContext)
		{
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Pattern(Value);
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = GetType().GetHashCode();
				hashCode = (hashCode * 397) ^ Value.GetHashCode();
				return hashCode;
			}
		}
	}
}