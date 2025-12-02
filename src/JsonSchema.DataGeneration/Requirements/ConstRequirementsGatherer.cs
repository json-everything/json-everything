using System.Linq;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class ConstRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var constKeyword = schema.GetKeyword<ConstKeyword>()?.RawValue;
		if (constKeyword != null)
		{
			if (context.ConstIsSet)
				context.HasConflict = true;
			else
			{
				context.Const = constKeyword.Value;
				context.ConstIsSet = true;
			}
		}
	}
}