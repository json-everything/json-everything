using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema.UniqueKeys;

public static class JsonSchemaExtensions
{
	public static IEnumerable<JsonPointer>? GetUniqueKeys(this JsonSchema schema)
	{
		return schema.TryGetKeyword<UniqueKeysKeyword>(UniqueKeysKeyword.Name, out var k) ? k!.Keys : null;
	}
}