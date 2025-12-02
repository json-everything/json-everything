using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class NumberRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var supportsNumbers = false;

		var range = NumberRangeSet.Full;
		var minimum = schema.GetKeyword<MinimumKeyword>()?.RawValue.GetDecimal();
		if (minimum != null)
		{
			range = range.Floor(minimum.Value);
			supportsNumbers = true;
		}
		minimum = schema.GetKeyword<ExclusiveMinimumKeyword>()?.RawValue.GetDecimal();
		if (minimum != null)
		{
			range = range.Floor((minimum.Value, false));
			supportsNumbers = true;
		}
		var maximum = schema.GetKeyword<MaximumKeyword>()?.RawValue.GetDecimal();
		if (maximum != null)
		{
			range = range.Ceiling(maximum.Value);
			supportsNumbers = true;
		}
		maximum = schema.GetKeyword<ExclusiveMaximumKeyword>()?.RawValue.GetDecimal();
		if (maximum != null)
		{
			range = range.Ceiling((maximum.Value, false));
			supportsNumbers = true;
		}
		if (range != NumberRangeSet.Full)
		{
			if (context.NumberRanges != null)
				context.NumberRanges *= range;
			else
				context.NumberRanges = range;
			supportsNumbers = true;
		}

		var multipleOf = schema.GetKeyword<MultipleOfKeyword>()?.RawValue.GetDecimal();
		if (multipleOf != null)
		{
			if (context.Multiples != null)
				context.Multiples?.Add(multipleOf.Value);
			else
				context.Multiples = [multipleOf.Value];
			supportsNumbers = true;
		}

		if (supportsNumbers)
			context.InferredType |= SchemaValueType.Number;
	}
}