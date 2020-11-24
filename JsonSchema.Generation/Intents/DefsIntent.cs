using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema.Generation.Intents
{
	internal class DefsIntent : ISchemaKeywordIntent
	{
		public Dictionary<string, SchemaGeneratorContext> Properties { get; }

		public DefsIntent(Dictionary<string, SchemaGeneratorContext> properties)
		{
			Properties = properties;
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
			builder.Defs(Properties.ToDictionary(p => p.Key, p => p.Value.Apply().Build()));
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
				foreach (var property in Properties)
				{
					hashCode = (hashCode * 397) ^ property.Key.GetHashCode();
					hashCode = (hashCode * 397) ^ property.Value.Intents.GetCollectionHashCode();
				}
				return hashCode;
			}
		}
	}
}