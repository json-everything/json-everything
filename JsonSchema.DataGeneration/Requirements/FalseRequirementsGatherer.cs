using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements;

[UsedImplicitly]
internal class FalseRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema)
	{
		if (schema.BoolValue == false)
			context.IsFalse = true;
	}
}