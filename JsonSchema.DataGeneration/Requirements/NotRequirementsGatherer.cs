using System.Linq;

namespace Json.Schema.DataGeneration.Requirements
{
	internal class NotRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementContext context, JsonSchema schema)
		{
			var notKeyword = schema.Keywords?.OfType<NotKeyword>().FirstOrDefault();
			if (notKeyword == null) return;

			var subRequirements = notKeyword.Schema.GetRequirements();
			
			context.And(subRequirements.Break());
		}
	}
}