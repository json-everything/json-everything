using System.Linq;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class EnumRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var enumKeyword = schema.GetKeyword<EnumKeyword>();
		if (enumKeyword != null)
		{
			if (context.EnumOptions != null)
				context.HasConflict = true;
			else
				context.EnumOptions = enumKeyword.RawValue.EnumerateArray().Select(x => (true, x)).ToList();
		}
	}
}