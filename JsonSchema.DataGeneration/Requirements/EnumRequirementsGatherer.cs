using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements;

[UsedImplicitly]
internal class EnumRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema)
	{
		var enumKeyword = schema.Keywords?.OfType<EnumKeyword>().FirstOrDefault();
		if (enumKeyword != null)
		{
			if (context.EnumOptions != null)
				context.HasConflict = true;
			else
				context.EnumOptions = enumKeyword.Values.ToList();
		}
	}
}