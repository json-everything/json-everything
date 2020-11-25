using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	public class PropertiesIntent : ISchemaKeywordIntent, IContextContainer
	{
		public Dictionary<string, SchemaGeneratorContext> Properties { get; }

		public PropertiesIntent(Dictionary<string, SchemaGeneratorContext> properties)
		{
			Properties = properties;
		}

		public IEnumerable<SchemaGeneratorContext> GetContexts()
		{
			return Properties.Values;
		}

		public void Replace(int hashCode, SchemaGeneratorContext newContext)
		{
			foreach (var property in Properties.ToList())
			{
				var hc = property.Value.GetHashCode();
				if (hc == hashCode)
					Properties[property.Key] = newContext;
			}
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Properties(Properties.ToDictionary(p => p.Key, p => p.Value.Apply().Build()));
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
					hashCode = (hashCode * 397) ^ property.Value.GetHashCode();
				}
				return hashCode;
			}
		}
	}
}