using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class NotRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var notKeyword = schema.Keywords?.OfType<NotKeyword>().FirstOrDefault();
			if (notKeyword == null) return;

			var subRequirements = notKeyword.Schema.GetRequirements();

			var broken = subRequirements.Break();
			context.And(broken);
		}
	}
}