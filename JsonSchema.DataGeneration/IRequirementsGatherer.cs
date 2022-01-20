namespace Json.Schema.DataGeneration
{
	internal interface IRequirementsGatherer
	{
		void AddRequirements(RequirementsContext context, JsonSchema schema);
	}
}