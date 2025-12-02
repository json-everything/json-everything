using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class AllOfRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var allOfKeyword = schema.GetKeyword<AllOfKeyword>();
		if (allOfKeyword == null) return;

		foreach (var subschema in allOfKeyword.Subschemas)
		{
			// ReSharper disable once IdentifierTypo
			var subrequirement = subschema.GetRequirements(options);
			context.And(subrequirement);
		}
	}
}