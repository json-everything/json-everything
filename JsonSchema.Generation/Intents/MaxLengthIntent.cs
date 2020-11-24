using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	internal class MaxLengthIntent : ISchemaKeywordIntent
	{
		public uint Value { get; }

		public MaxLengthIntent(uint value)
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
			builder.MaxLength(Value);
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