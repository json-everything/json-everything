namespace Json.Schema.Generation
{
	internal interface IAttributeHandler
	{
		void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context);
	}
}