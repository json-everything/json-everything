using System.Linq;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class OneOfRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var keyword = schema.GetKeyword<OneOfKeyword>();
		if (keyword == null) return;

		context.Options ??= [];
		var allRequirements = keyword.Subschemas.Select(x => x.GetRequirements(options)).ToList();
		var inverted = allRequirements.Select(x => x.Break()).ToList();

		var i = 0;
		while (i < allRequirements.Count)
		{
			var subRequirement = new RequirementsContext(allRequirements[i]);
			// ReSharper disable once AccessToModifiedClosure
			var othersInverted = inverted.Where((_, j) => i != j);

			foreach (var inversion in othersInverted)
			{
				subRequirement.And(inversion);
			}

			context.Options.Add(subRequirement);
			i++;
		}
	}
}