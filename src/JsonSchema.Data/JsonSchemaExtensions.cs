using System.Collections.Generic;
using Json.Schema.Data.Keywords;

namespace Json.Schema.Data;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static class JsonSchemaExtensions
{
	/// <summary>
	/// Gets the references in `data` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, IDataResourceIdentifier>? GetData(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DataKeyword>(DataKeyword.Name, out var k) ? k.References : null;
	}

	/// <summary>
	/// Gets the references in `optionalData` if the keyword exists.
	/// </summary>
	public static IReadOnlyDictionary<string, IDataResourceIdentifier>? GetOptionalData(this JsonSchema schema)
	{
		return schema.TryGetKeyword<OptionalDataKeyword>(OptionalDataKeyword.Name, out var k) ? k.References : null;
	}
}