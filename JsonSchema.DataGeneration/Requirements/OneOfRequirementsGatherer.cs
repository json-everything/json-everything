using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class OneOfRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var keyword = schema.Keywords?.OfType<OneOfKeyword>().FirstOrDefault();
			if (keyword == null) return;

			context.Options ??= new List<RequirementsContext>();
			var allRequirements = keyword.Schemas.Select(x => x.GetRequirements()).ToList();
			var inverted = allRequirements.Select(x => x.Break()).ToList();

			var i = 0;
			while (i < allRequirements.Count)
			{
				var subRequirement = new RequirementsContext(allRequirements[i]);
				var othersInverted = inverted.Where((_, j) => i != j);

				foreach (var inversion in othersInverted)
				{
					subRequirement.And(inversion);
				}

				context.Options.Add(subRequirement);
				i++;
			}
		}
	}
}