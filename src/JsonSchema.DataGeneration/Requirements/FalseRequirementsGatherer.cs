using System.Text.Json;

namespace Json.Schema.DataGeneration.Requirements;

internal class FalseRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		if (schema.Source.ValueKind is JsonValueKind.False)
			context.IsFalse = true;
	}
}