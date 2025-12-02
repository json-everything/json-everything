using System.Linq;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class AnyOfRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var keyword = schema.GetKeyword<AnyOfKeyword>();
		if (keyword == null) return;

		context.Options ??= [];
		context.Options.AddRange(keyword.Subschemas.Select(x => x.GetRequirements(options)));
	}
}