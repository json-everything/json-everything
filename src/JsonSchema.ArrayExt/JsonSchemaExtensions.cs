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
	public static IEnumerable<JsonPointer_Old>? GetUniqueKeys(this JsonSchema schema)
	{
		return schema.TryGetKeyword<UniqueKeysKeyword>(UniqueKeysKeyword.Name, out var k) ? k.Keys : null;
	}
	/// <summary>
	/// Gets the values in `uniqueKeys` if the keyword exists.
	/// </summary>
	public static IEnumerable<OrderingSpecifier>? GetOrdering(this JsonSchema schema)
	{
		return schema.TryGetKeyword<OrderingKeyword>(OrderingKeyword.Name, out var k) ? k.Specifiers : null;
	}
}