using System.Linq;

namespace Json.Schema.Generation
{
	internal static class AttributeHandler
	{
		internal static void HandleAttributes(SchemaGeneratorContext context)
		{
			foreach (var handler in context.Attributes.OfType<IAttributeHandler>())
			{
				handler.AddConstraints(context);
			}
		}
	}
}