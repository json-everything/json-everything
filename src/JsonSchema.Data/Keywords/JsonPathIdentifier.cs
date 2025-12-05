using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Path;

namespace Json.Schema.Data.Keywords;

/// <summary>
/// Handles data references that are JSON Paths.
/// </summary>
public class JsonPathIdentifier : IDataResourceIdentifier
{
	/// <summary>
	/// Gets the JSON Path query.
	/// </summary>
	public JsonPath Query { get; }

	/// <summary>
	/// Creates a new <see cref="JsonPathIdentifier"/>.
	/// </summary>
	/// <param name="query"></param>
	public JsonPathIdentifier(JsonPath query)
	{
		Query = query;
	}

	/// <summary>
	/// Attempts to resolve a value from the specified JSON element.
	/// </summary>
	/// <param name="root">The root <see cref="JsonElement"/> to search for the desired value.</param>
	/// <param name="value">When this method returns, contains the resolved <see cref="JsonElement"/> if the operation succeeds; otherwise,
	/// contains the default value.</param>
	/// <returns>true if the value was successfully resolved; otherwise, false.</returns>
	public bool TryResolve(JsonElement root, out JsonElement value)
	{
		var results = Query.Evaluate(root.AsNode());

		value = JsonSerializer.SerializeToElement(results.Matches.Select(x => x.Value), JsonSchemaDataSerializerContext.Default.IEnumerableJsonNode!);
		return true;
	}
}