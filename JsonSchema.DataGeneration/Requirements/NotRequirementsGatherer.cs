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
			
			context.And(Invert(subRequirements));
		}

		private static RequirementContext Invert(RequirementContext context)
		{
			return new RequirementContext
			{
				NumberRanges = context.NumberRanges?.Invert(),
				Multiples = context.Antimultiples,
				Antimultiples = context.Multiples
			};
		}
	}
}