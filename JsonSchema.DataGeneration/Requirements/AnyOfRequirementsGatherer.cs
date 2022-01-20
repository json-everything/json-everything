using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration.Requirements
{
	internal class AnyOfRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var keyword = schema.Keywords?.OfType<AnyOfKeyword>().FirstOrDefault();
			if (keyword == null) return;

			context.Options ??= new List<RequirementsContext>();
			context.Options.AddRange(keyword.Schemas.Select(x => x.GetRequirements()));
		}
	}
}