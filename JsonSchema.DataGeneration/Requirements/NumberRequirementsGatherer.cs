using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class NumberRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var range = NumberRangeSet.Full;
			var minimum = schema.Keywords?.OfType<MinimumKeyword>().FirstOrDefault()?.Value;
			if (minimum != null)
				range = range.Floor(minimum.Value);
			var maximum = schema.Keywords?.OfType<MaximumKeyword>().FirstOrDefault()?.Value;
			if (maximum != null)
				range = range.Ceiling(maximum.Value);
			if (range != NumberRangeSet.Full)
			{
				if (context.NumberRanges != null)
					context.NumberRanges *= range;
				else
					context.NumberRanges = range;
			}

			var multipleOf = schema.Keywords?.OfType<MultipleOfKeyword>().FirstOrDefault()?.Value;
			if (multipleOf != null)
			{
				if (context.Multiples != null)
					context.Multiples?.Add(multipleOf.Value);
				else
					context.Multiples = new List<decimal> {multipleOf.Value};
			}
		}
	}
}