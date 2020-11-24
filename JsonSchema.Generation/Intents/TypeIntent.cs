using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	internal class TypeIntent : ISchemaKeywordIntent
	{
		public SchemaValueType Type { get; }

		public TypeIntent(SchemaValueType type)
		{
			Type = type;
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
			builder.Type(Type);
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
				hashCode = (hashCode * 397) ^ Type.GetHashCode();
				return hashCode;
			}
		}
	}
}