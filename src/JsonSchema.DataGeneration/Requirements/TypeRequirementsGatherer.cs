using System.Linq;

namespace Json.Schema.DataGeneration.Requirements;

internal class TypeRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema, EvaluationOptions options)
	{
		var typeKeyword = schema.Keywords?.OfType<TypeKeyword>().FirstOrDefault();
		if (typeKeyword == null) return;

		if (context.Type == null)
			context.Type = typeKeyword.Type;
		else
			context.Type &= typeKeyword.Type;
	}
}