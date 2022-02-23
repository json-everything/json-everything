using System;
using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class StringRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var range = NumberRangeSet.NonNegative;
			var minLength = schema.Keywords?.OfType<MinLengthKeyword>().FirstOrDefault()?.Value;
			if (minLength != null)
				range = range.Floor(minLength.Value);
			var maxLength = schema.Keywords?.OfType<MaxLengthKeyword>().FirstOrDefault()?.Value;
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

			//var pattern = schema.Keywords!.OfType<PatternKeyword>().FirstOrDefault()?.Value;
			//if (pattern != null)
			//{
			//	context.Patterns ??= new List<Regex>();
			//	context.Patterns.Add(pattern);
			//}

			if (context.Format != null)
				context.HasConflict = true;
			else
				context.Format = schema.Keywords?.OfType<FormatKeyword>().FirstOrDefault()?.Value.Key;
		}
	}
}