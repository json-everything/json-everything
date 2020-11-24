using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	internal class DefsIntent : ISchemaKeywordIntent
	{
		public Dictionary<string, SchemaGeneratorContext> Definitions { get; }

		public DefsIntent(Dictionary<string, SchemaGeneratorContext> properties)
		{
			Definitions = properties;
		}

		public IEnumerable<SchemaGeneratorContext> GetChildContexts()
		{
			throw new System.NotImplementedException("Shouldn't be optimizing a def.");
		}

		public void Replace(int hashCode, SchemaGeneratorContext newContext)
		{
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Defs(Definitions.ToDictionary(p => p.Key, p => p.Value.Apply().Build()));
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
				foreach (var property in Definitions)
				{
					hashCode = (hashCode * 397) ^ property.Key.GetHashCode();
					hashCode = (hashCode * 397) ^ property.Value.Attributes.GetTypeBasedHashCode();
				}
				return hashCode;
			}
		}
	}
}