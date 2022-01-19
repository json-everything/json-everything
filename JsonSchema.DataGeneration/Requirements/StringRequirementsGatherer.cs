using System.Linq;

namespace Json.Schema.DataGeneration.Requirements
{
	internal class StringRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementContext context, JsonSchema schema)
		{
			var range = NumberRangeSet.NonNegative;
			var minLength = schema.Keywords!.OfType<MinLengthKeyword>().FirstOrDefault()?.Value;
			if (minLength != null)
				range = range.Floor(minLength.Value);
			var maxLength = schema.Keywords!.OfType<MaxLengthKeyword>().FirstOrDefault()?.Value;
			if (maxLength != null)
				range = range.Ceiling(maxLength.Value);
			if (range != NumberRangeSet.NonNegative)
			{
				if (context.StringLengths != null)
					context.StringLengths *= range;
				else
				{
					context.StringLengths = range;
				}
			}

		}
	}
}