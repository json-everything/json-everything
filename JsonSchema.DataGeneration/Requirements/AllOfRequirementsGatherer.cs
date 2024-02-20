using System.Linq;

namespace Json.Schema.DataGeneration.Requirements;

internal class AllOfRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema, EvaluationOptions options)
	{
		var allOfKeyword = schema.Keywords?.OfType<AllOfKeyword>().FirstOrDefault();
		if (allOfKeyword == null) return;

		foreach (var subschema in allOfKeyword.Schemas)
		{
			// ReSharper disable once IdentifierTypo
			var subrequirement = subschema.GetRequirements(options);
			context.And(subrequirement);
		}
	}
}