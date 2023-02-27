using System.Collections.Generic;

namespace Json.Schema.Data;

public static class JsonSchemaExtensions
{
	public static IReadOnlyDictionary<string, IDataResourceIdentifier>? GetData(this JsonSchema schema)
	{
		return schema.TryGetKeyword<DataKeyword>(DataKeyword.Name, out var k) ? k!.References : null;
	}
}