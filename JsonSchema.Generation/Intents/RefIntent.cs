using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	internal class RefIntent : ISchemaKeywordIntent
	{
		public Uri Reference { get; }

		public RefIntent(Uri reference)
		{
			Reference = reference;
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
			builder.Ref(Reference);
		}
	}
}