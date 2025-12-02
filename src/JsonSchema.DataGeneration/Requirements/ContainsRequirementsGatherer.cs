using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class ContainsRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var contains = schema.GetKeyword<ContainsKeyword>() ??
		               schema.GetKeyword<Keywords.Draft06.ContainsKeyword>();
		if (contains != null)
		{
			if (context.Contains != null)
				context.Contains.And(contains.Subschemas[0].GetRequirements(options));
			else
				context.Contains = contains.Subschemas[0].GetRequirements(options);
		}

		var range = NumberRangeSet.Full;
		var minimum = (long?)schema.GetKeyword<MinContainsKeyword>()?.Value;
		if (minimum != null)
			range = range.Floor(minimum.Value);
		var maximum = (long?)schema.GetKeyword<MaxContainsKeyword>()?.Value;
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