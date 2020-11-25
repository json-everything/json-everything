namespace Json.Schema.Generation
{
	public interface IAttributeHandler
	{
		void AddConstraints(SchemaGeneratorContext context);
	}
}