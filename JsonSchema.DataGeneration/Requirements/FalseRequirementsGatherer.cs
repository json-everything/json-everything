namespace Json.Schema.DataGeneration.Requirements
{
	internal class FalseRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			if (schema.BoolValue == false)
				context.IsFalse = true;
		}
	}
}