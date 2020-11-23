using System.Collections.Generic;
using System.Linq;
using Json.More;

namespace Json.Schema.Generation.Intents
{
	internal class EnumIntent : ISchemaKeywordIntent
	{
		public List<string> Names { get; set; }

		public EnumIntent(List<string> names)
		{
			Names = names;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Enum(Names.Select(n => n.AsJsonElement()));
		}
	}
}