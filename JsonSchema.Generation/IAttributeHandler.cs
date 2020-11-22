using System.Reflection;

namespace Json.Schema.Generation
{
	internal interface IAttributeHandler
	{
		void AddConstraints(JsonSchemaBuilder objectBuilder, JsonSchemaBuilder propertyBuilder, PropertyInfo property);
	}
}