using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class PropertiesRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var supportsObjects = false;

		var range = NumberRangeSet.Full;
		var minimum = (long?)schema.GetKeyword<MinPropertiesKeyword>()?.Value;
		if (minimum != null)
		{
			range = range.Floor(minimum.Value);
			supportsObjects = true;
		}
		var maximum = (long?)schema.GetKeyword<MaxPropertiesKeyword>()?.Value;
		if (maximum != null)
		{
			range = range.Ceiling(maximum.Value);
			supportsObjects = true;
		}
		if (range != NumberRangeSet.Full)
		{
			if (context.PropertyCounts != null)
				context.PropertyCounts *= range;
			else
				context.PropertyCounts = range;
			supportsObjects = true;
		}

		var requiredProperties = (string[]?)schema.GetKeyword<RequiredKeyword>()?.Value;
		if (requiredProperties != null)
		{
			if (context.RequiredProperties != null)
				context.RequiredProperties.AddRange(requiredProperties);
			else
				context.RequiredProperties = [.. requiredProperties];
			supportsObjects = true;
		}

		var properties = schema.GetKeyword<PropertiesKeyword>();
		if (properties != null)
		{
			context.Properties ??= [];
			foreach (var property in properties.Subschemas)
			{
				var propertyName = property.RelativePath[0].ToString();
				if (context.Properties.TryGetValue(propertyName, out var subschema))
					subschema.And(property.GetRequirements(options));
				else
					context.Properties.Add(propertyName, property.GetRequirements(options));
			}
			supportsObjects = true;
		}

		var additionalProperties = schema.GetKeyword<AdditionalPropertiesKeyword>();
		if (additionalProperties != null)
		{
			if (context.RemainingProperties != null)
				context.RemainingProperties.And(additionalProperties.Subschemas[0].GetRequirements(options));
			else
				context.RemainingProperties = additionalProperties.Subschemas[0].GetRequirements(options);
			supportsObjects = true;
		}

		additionalProperties = schema.GetKeyword<UnevaluatedPropertiesKeyword>();
		if (additionalProperties != null)
		{
			if (context.RemainingProperties != null)
				context.RemainingProperties.And(additionalProperties.Subschemas[0].GetRequirements(options));
			else
				context.RemainingProperties = additionalProperties.Subschemas[0].GetRequirements(options);
			supportsObjects = true;
		}

		if (supportsObjects)
			context.InferredType |= SchemaValueType.Object;
	}
}