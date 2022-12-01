using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema;

public interface ICustomSchemaCollector
{
	IEnumerable<JsonSchema> Schemas { get; }

	(JsonSchema?, int) FindSubschema(IReadOnlyList<PointerSegment> segments);
}