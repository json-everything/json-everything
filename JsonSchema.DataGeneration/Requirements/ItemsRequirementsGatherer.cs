using System.Linq;

namespace Json.Schema.DataGeneration.Requirements;

internal class ItemsRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchema schema, EvaluationOptions options)
	{
		var supportsArrays = false;

		var range = NumberRangeSet.Full;
		var minimum = schema.Keywords?.OfType<MinItemsKeyword>().FirstOrDefault()?.Value;
		if (minimum != null)
		{
			range = range.Floor(minimum.Value);
			supportsArrays = true;
		}
		var maximum = schema.Keywords?.OfType<MaxItemsKeyword>().FirstOrDefault()?.Value;
		if (maximum != null)
		{
			range = range.Ceiling(maximum.Value);
			supportsArrays = true;
		}
		if (range != NumberRangeSet.Full)
		{
			if (context.ItemCounts != null)
				context.ItemCounts *= range;
			else
				context.ItemCounts = range;
		}

		var items = schema.Keywords?.OfType<ItemsKeyword>().FirstOrDefault();
		if (items != null)
		{
			if (items.SingleSchema != null)
			{
				if (context.RemainingItems != null)
					context.RemainingItems.And(items.SingleSchema.GetRequirements(options));
				else
					context.RemainingItems = items.SingleSchema.GetRequirements(options);
			}
			else
			{
				if (context.SequentialItems != null)
				{
					// need to AND the schemas together sequentially
				}
				else
					context.SequentialItems = items.ArraySchemas!.Select(x => x.GetRequirements(options)).ToList();
			}
			supportsArrays = true;
		}

		var prefixItems = schema.Keywords?.OfType<PrefixItemsKeyword>().FirstOrDefault()?.ArraySchemas;
		if (prefixItems != null)
		{
			if (context.SequentialItems != null)
			{
				// need to AND the schemas together sequentially
			}
			else
				context.SequentialItems = prefixItems.Select(x => x.GetRequirements(options)).ToList();
			supportsArrays = true;
		}

		var additionalItems = schema.Keywords?.OfType<AdditionalItemsKeyword>().FirstOrDefault()?.Schema;
		if (additionalItems != null)
		{
			if (context.RemainingItems != null)
				context.RemainingItems.And(additionalItems.GetRequirements(options));
			else
				context.RemainingItems = additionalItems.GetRequirements(options);
			supportsArrays = true;
		}

		additionalItems = schema.Keywords?.OfType<UnevaluatedItemsKeyword>().FirstOrDefault()?.Schema;
		if (additionalItems != null)
		{
			if (context.RemainingItems != null)
				context.RemainingItems.And(additionalItems.GetRequirements(options));
			else
				context.RemainingItems = additionalItems.GetRequirements(options);
			supportsArrays = true;
		}

		if (supportsArrays)
			context.InferredType |= SchemaValueType.Array;
	}
}