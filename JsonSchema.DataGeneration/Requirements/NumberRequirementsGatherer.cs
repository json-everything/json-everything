using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration.Requirements
{
	internal class NumberRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var range = NumberRangeSet.Full;
			var minimum = schema.Keywords!.OfType<MinimumKeyword>().FirstOrDefault()?.Value;
			if (minimum != null)
				range = range.Floor(minimum.Value);
			var maximum = schema.Keywords!.OfType<MaximumKeyword>().FirstOrDefault()?.Value;
			if (maximum != null)
				range = range.Ceiling(maximum.Value);
			if (range != NumberRangeSet.Full)
			{
				if (context.NumberRanges != null)
					context.NumberRanges *= range;
				else
					context.NumberRanges = range;
			}

			var multipleOf = schema.Keywords!.OfType<MultipleOfKeyword>().FirstOrDefault()?.Value;
			if (multipleOf != null)
			{
				if (context.Multiples != null)
					context.Multiples?.Add(multipleOf.Value);
				else
					context.Multiples = new List<decimal> {multipleOf.Value};
			}
		}
	}

	internal class ItemsRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var range = NumberRangeSet.Full;
			var minimum = schema.Keywords!.OfType<MinItemsKeyword>().FirstOrDefault()?.Value;
			if (minimum != null)
				range = range.Floor(minimum.Value);
			var maximum = schema.Keywords!.OfType<MaxItemsKeyword>().FirstOrDefault()?.Value;
			if (maximum != null)
				range = range.Ceiling(maximum.Value);
			if (range != NumberRangeSet.Full)
			{
				if (context.ItemCounts != null)
					context.ItemCounts *= range;
				else
					context.ItemCounts = range;
			}

			var items = schema.Keywords!.OfType<ItemsKeyword>().FirstOrDefault();
			if (items != null)
			{
				if (items.SingleSchema != null)
				{
					if (context.RemainingItems != null)
						context.RemainingItems.And(items.SingleSchema.GetRequirements());
					else
						context.RemainingItems = items.SingleSchema.GetRequirements();
				}
				else
				{
					if (context.SequentialItems != null)
					{
						// need to AND the schemas together sequentially
					}
					else
						context.SequentialItems = items.ArraySchemas!.Select(x => x.GetRequirements()).ToList();
				}
			}

			var prefixItems = schema.Keywords!.OfType<PrefixItemsKeyword>().FirstOrDefault();
			if (prefixItems != null)
			{
				if (context.SequentialItems != null)
				{
					// need to AND the schemas together sequentially
				}
				else
					context.SequentialItems = prefixItems.ArraySchemas!.Select(x => x.GetRequirements()).ToList();
			}

			var additionalItems = schema.Keywords!.OfType<AdditionalItemsKeyword>().FirstOrDefault();
			if (prefixItems != null)
			{
				if (context.RemainingItems != null)
					context.RemainingItems.And(additionalItems.Schema.GetRequirements());
				else
					context.RemainingItems = additionalItems.Schema.GetRequirements();
			}
		}
	}
}