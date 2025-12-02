using System.Linq;
using System.Text.Json;
using Json.Schema.Keywords;
using Json.Schema.Keywords.Draft06;
using ItemsKeyword = Json.Schema.Keywords.ItemsKeyword;

namespace Json.Schema.DataGeneration.Requirements;

internal class ItemsRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var supportsArrays = false;

		var range = NumberRangeSet.Full;
		var minimum = (long?)schema.GetKeyword<MinItemsKeyword>()?.Value;
		if (minimum != null)
		{
			range = range.Floor(minimum.Value);
			supportsArrays = true;
		}
		var maximum = (long?)schema.GetKeyword<MaxItemsKeyword>()?.Value;
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

		var items = schema.GetKeyword<ItemsKeyword>();
		if (items != null)
		{
			if (items.RawValue.ValueKind is JsonValueKind.Object or JsonValueKind.True or JsonValueKind.False)
			{
				if (context.RemainingItems != null)
					context.RemainingItems.And(items.Subschemas[0].GetRequirements(options));
				else
					context.RemainingItems = items.Subschemas[0].GetRequirements(options);
			}
			else
			{
				if (context.SequentialItems != null)
				{
					// need to AND the schemas together sequentially
				}
				else
					context.SequentialItems = items.Subschemas.Select(x => x.GetRequirements(options)).ToList();
			}
			supportsArrays = true;
		}

		var prefixItems = schema.GetKeyword<PrefixItemsKeyword>();
		if (prefixItems != null)
		{
			if (context.SequentialItems != null)
			{
				// need to AND the schemas together sequentially
			}
			else
				context.SequentialItems = prefixItems.Subschemas.Select(x => x.GetRequirements(options)).ToList();
			supportsArrays = true;
		}

		var additionalItems = schema.GetKeyword<AdditionalItemsKeyword>();
		if (additionalItems != null)
		{
			if (context.RemainingItems != null)
				context.RemainingItems.And(additionalItems.Subschemas[0].GetRequirements(options));
			else
				context.RemainingItems = additionalItems.Subschemas[0].GetRequirements(options);
			supportsArrays = true;
		}

		additionalItems = schema.GetKeyword<UnevaluatedItemsKeyword>();
		if (additionalItems != null)
		{
			if (context.RemainingItems != null)
				context.RemainingItems.And(additionalItems.Subschemas[0].GetRequirements(options));
			else
				context.RemainingItems = additionalItems.Subschemas[0].GetRequirements(options);
			supportsArrays = true;
		}

		if (supportsArrays)
			context.InferredType |= SchemaValueType.Array;
	}
}