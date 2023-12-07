using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using Json.Path;

namespace Json.Schema.Data;

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
	/// Resolves a resource.
	/// </summary>
	/// <param name="evaluation">The evaluation being process.  This will help identify.</param>
	/// <param name="registry">The schema registry.</param>
	/// <param name="value">The value, if <paramref name="evaluation"/> was resolvable.</param>
	/// <returns>True if resolution was successful; false otherwise.</returns>
	public bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value)
	{
		var results = Query.Evaluate(evaluation.LocalInstance);
		if (results.Error == null)
		{
			value = null;
			return false;
		}

		value = results.Matches!.Select(x => x.Value).ToJsonArray();
		return true;
	}
}