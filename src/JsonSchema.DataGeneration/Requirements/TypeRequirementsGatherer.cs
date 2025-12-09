using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class TypeRequirementsGatherer : IRequirementsGatherer
{
	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema, BuildOptions options)
	{
		var typeKeyword = schema.GetKeyword<TypeKeyword>();
		if (typeKeyword == null) return;

		if (context.Type == null)
			context.Type = (SchemaValueType)typeKeyword.Value!;
		else
			context.Type &= (SchemaValueType)typeKeyword.Value!;
	}
}