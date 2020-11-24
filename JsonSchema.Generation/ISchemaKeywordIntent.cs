using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public interface ISchemaKeywordIntent
	{
		IEnumerable<SchemaGeneratorContext> GetChildContexts();
		void Replace(int hashCode, SchemaGeneratorContext newContext);
		void Apply(JsonSchemaBuilder builder);
	}
}