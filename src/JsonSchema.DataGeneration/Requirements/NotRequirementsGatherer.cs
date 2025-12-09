using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class NotRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var notKeyword = schema.GetKeyword<NotKeyword>();
		if (notKeyword == null) return;

		var subRequirements = notKeyword.Subschemas[0].GetRequirements(options);

		var broken = subRequirements.Break();
		context.And(broken);
	}
}