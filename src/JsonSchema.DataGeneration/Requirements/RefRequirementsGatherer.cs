using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class RefRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var refKeyword = schema.GetKeyword<RefKeyword>();
		if (refKeyword != null)
		{
			if (refKeyword.Subschemas is null or { Length: 0 })
				throw new RefResolutionException((Uri)refKeyword.Value!);

			if (context.RemainingProperties != null)
				context.RemainingProperties.And(refKeyword.Subschemas[0].GetRequirements(options));
			else
				context.RemainingProperties = refKeyword.Subschemas[0].GetRequirements(options);
		}
	}
}