﻿using System.Linq;

namespace Json.Schema.DataGeneration.Requirements;

internal class StringRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema, EvaluationOptions options)
	{
		var supportsStrings = false;

		var range = NumberRangeSet.NonNegative;
		var minLength = schema.Keywords?.OfType<MinLengthKeyword>().FirstOrDefault()?.Value;
		if (minLength != null)
		{
			range = range.Floor(minLength.Value);
			supportsStrings = true;
		}
		var maxLength = schema.Keywords?.OfType<MaxLengthKeyword>().FirstOrDefault()?.Value;
		if (maxLength != null)
		{
			range = range.Ceiling(maxLength.Value);
			supportsStrings = true;
		}
		if (range != NumberRangeSet.NonNegative)
		{
			if (context.StringLengths != null)
				context.StringLengths *= range;
			else
			{
				context.StringLengths = range;
			}
			supportsStrings = true;
		}

		var pattern = schema.Keywords?.OfType<PatternKeyword>().FirstOrDefault()?.Pattern;
		if (pattern != null)
		{
			//context.Patterns ??= new List<Regex>();
			//context.Patterns.Add(pattern);
			context.Pattern = pattern;
		}

		if (context.Format != null)
			context.HasConflict = true;
		else
		{
			context.Format = schema.Keywords?.OfType<FormatKeyword>().FirstOrDefault()?.Value.Key;
			supportsStrings = context.Format != null;
		}

		if (supportsStrings)
			context.InferredType |= SchemaValueType.String;
	}
}