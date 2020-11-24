using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	internal class DeprecatedIntent : ISchemaKeywordIntent
	{
		public bool Deprecated { get; }

		public DeprecatedIntent(bool deprecated)
		{
			Deprecated = deprecated;
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
			builder.Deprecated(Deprecated);
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
				hashCode = (hashCode * 397) ^ Deprecated.GetHashCode();
				return hashCode;
			}
		}
	}
}