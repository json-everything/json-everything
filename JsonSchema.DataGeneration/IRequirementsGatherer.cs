namespace Json.Schema.DataGeneration
{
	internal interface IRequirementsGatherer
	{
		void AddRequirements(RequirementContext context, JsonSchema schema);
	}
}