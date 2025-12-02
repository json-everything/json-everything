using System.Linq;
using System.Text.RegularExpressions;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class StringRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var supportsStrings = false;

		var range = NumberRangeSet.NonNegative;
		var minLength = schema.GetKeyword<MinLengthKeyword>()?.RawValue.GetDecimal();
		if (minLength != null)
		{
			range = range.Floor(minLength.Value);
			supportsStrings = true;
		}
		var maxLength = schema.GetKeyword<MaxLengthKeyword>()?.RawValue.GetDecimal();
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

		var pattern = (Regex?)schema.GetKeyword<PatternKeyword>()?.Value;
		if (pattern != null)
		{
			//context.Patterns ??= new List<Regex>();
			//context.Patterns.Add(pattern);
			context.Pattern = pattern.ToString();
		}

		if (context.Format != null)
			context.HasConflict = true;
		else
		{
			context.Format = schema.GetKeyword<FormatKeyword>()?.RawValue.GetString() ??
			                 schema.GetKeyword<Keywords.Draft06.FormatKeyword>()?.RawValue.GetString();
			supportsStrings = context.Format != null;
		}

		if (supportsStrings)
			context.InferredType |= SchemaValueType.String;
	}
}