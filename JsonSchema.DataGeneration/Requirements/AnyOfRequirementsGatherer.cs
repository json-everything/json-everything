using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements;

[UsedImplicitly]
internal class AnyOfRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema)
	{
		var keyword = schema.Keywords?.OfType<AnyOfKeyword>().FirstOrDefault();
		if (keyword == null) return;

		context.Options ??= [];
		context.Options.AddRange(keyword.Schemas.Select(x => x.GetRequirements()));
	}
}