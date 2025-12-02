using System.Linq;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class ConditionalRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var ifKeyword = schema.GetKeyword<IfKeyword>();
		var thenKeyword = schema.GetKeyword<ThenKeyword>();
		var elseKeyword = schema.GetKeyword<ElseKeyword>();

		if (ifKeyword != null)
		{
			RequirementsContext? ifthen = null;
			if (thenKeyword != null)
			{
				ifthen = ifKeyword.Subschemas[0].GetRequirements(options);
				ifthen.And(thenKeyword.Subschemas[0].GetRequirements(options));
			}

			RequirementsContext? ifelse = null;
			if (elseKeyword != null)
			{
				ifelse = ifKeyword.Subschemas[0].GetRequirements(options).Break();
				ifelse.And(elseKeyword.Subschemas[0].GetRequirements(options));
			}

			if (ifthen == null && ifelse == null) return;
			if (ifthen == null)
				context.And(ifelse!);
			else if (ifelse == null)
				context.And(ifthen);
			else
			{
				if (context.Options != null)
				{
					context.Options.Add(ifthen);
					context.Options.Add(ifelse);
				}
				else
					context.Options = [ifthen, ifelse];
			}
		}
	}
}