using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static class JsonSchemaExtensions
{
	/// <summary>
	/// Gets the values in `uniqueKeys` if the keyword exists.
	/// </summary>
	public static IEnumerable<JsonPointer>? GetUniqueKeys(this JsonSchema schema)
	{
		return schema.TryGetKeyword<UniqueKeysKeyword>(UniqueKeysKeyword.Name, out var k) ? k!.Keys : null;
	}
}