using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public interface IContextContainer
	{
		IEnumerable<SchemaGeneratorContext> GetContexts();
		void Replace(int hashCode, SchemaGeneratorContext newContext);
	}
}