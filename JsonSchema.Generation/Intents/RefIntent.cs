using System;

namespace Json.Schema.Generation.Intents
{
	internal class RefIntent : ISchemaKeywordIntent
	{
		public Uri Reference { get; }

		public RefIntent(Uri reference)
		{
			Reference = reference;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Ref(Reference);
		}
	}
}