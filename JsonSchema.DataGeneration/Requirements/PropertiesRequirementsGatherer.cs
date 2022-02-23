using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class PropertiesRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var range = NumberRangeSet.Full;
			var minimum = schema.Keywords?.OfType<MinPropertiesKeyword>().FirstOrDefault()?.Value;
			if (minimum != null)
				range = range.Floor(minimum.Value);
			var maximum = schema.Keywords?.OfType<MaxPropertiesKeyword>().FirstOrDefault()?.Value;
			if (maximum != null)
				range = range.Ceiling(maximum.Value);
			if (range != NumberRangeSet.Full)
			{
				if (context.PropertyCounts != null)
					context.PropertyCounts *= range;
				else
					context.PropertyCounts = range;
			}

			var requiredProperties = schema.Keywords?.OfType<RequiredKeyword>().FirstOrDefault()?.Properties;
			if (requiredProperties != null)
			{
				if (context.RequiredProperties != null)
					context.RequiredProperties.AddRange(requiredProperties);
				else
					context.RequiredProperties = requiredProperties.ToList();
			}

			var properties = schema.Keywords?.OfType<PropertiesKeyword>().FirstOrDefault();
			if (properties != null)
			{
				context.Properties ??= new Dictionary<string, RequirementsContext>();
				foreach (var property in properties.Properties)
				{
					if (context.Properties.TryGetValue(property.Key, out var subschema))
						subschema.And(property.Value.GetRequirements());
					else
						context.Properties.Add(property.Key, property.Value.GetRequirements());
				}
			}

			var additionalProperties = schema.Keywords?.OfType<AdditionalPropertiesKeyword>().FirstOrDefault()?.Schema;
			if (additionalProperties != null)
			{
				if (context.RemainingProperties != null)
					context.RemainingProperties.And(additionalProperties.GetRequirements());
				else
					context.RemainingProperties = additionalProperties.GetRequirements();
			}

			additionalProperties = schema.Keywords?.OfType<UnevaluatedPropertiesKeyword>().FirstOrDefault()?.Schema;
			if (additionalProperties != null)
			{
				if (context.RemainingProperties != null)
					context.RemainingProperties.And(additionalProperties.GetRequirements());
				else
					context.RemainingProperties = additionalProperties.GetRequirements();
			}
		}
	}
}