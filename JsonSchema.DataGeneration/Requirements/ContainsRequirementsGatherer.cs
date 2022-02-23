using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class ContainsRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var contains = schema.Keywords?.OfType<ContainsKeyword>().FirstOrDefault()?.Schema;
			if (contains != null)
			{
				if (context.Contains != null)
					context.Contains.And(contains.GetRequirements());
				else
					context.Contains = contains.GetRequirements();
			}

			var range = NumberRangeSet.Full;
			var minimum = schema.Keywords?.OfType<MinContainsKeyword>().FirstOrDefault()?.Value;
			if (minimum != null)
				range = range.Floor(minimum.Value);
			var maximum = schema.Keywords?.OfType<MaxContainsKeyword>().FirstOrDefault()?.Value;
			if (maximum != null)
				range = range.Ceiling(maximum.Value);
			if (range != NumberRangeSet.Full)
			{
				if (context.ContainsCounts != null)
					context.ContainsCounts *= range;
				else
					context.ContainsCounts = range;
			}
		}
	}
}