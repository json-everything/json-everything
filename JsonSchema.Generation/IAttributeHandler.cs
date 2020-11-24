namespace Json.Schema.Generation
{
	internal interface IAttributeHandler
	{
		void AddConstraints(SchemaGeneratorContext context);
	}
}