using System.Collections.Generic;

namespace Json.Schema.Generation.Intents
{
	internal class AdditionalPropertiesIntent : ISchemaKeywordIntent, IContextContainer
	{
		public SchemaGeneratorContext Context { get; private set; }

		public AdditionalPropertiesIntent(SchemaGeneratorContext context)
		{
			Context = context;
		}

		public IEnumerable<SchemaGeneratorContext> GetContexts()
		{
			return new[] {Context};
		}

		public void Replace(int hashCode, SchemaGeneratorContext newContext)
		{
			var hc = Context.GetHashCode();
			if (hc == hashCode)
				Context = newContext;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.AdditionalProperties(Context.Apply());
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = typeof(AdditionalPropertiesIntent).GetHashCode();
				hashCode = (hashCode * 397) ^ Context.Attributes.GetTypeBasedHashCode();
				return hashCode;
			}
		}
	}
}