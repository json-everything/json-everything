using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Implements the `count()` function which returns the number of nodes
/// in a nodelist.
/// </summary>
public class CountFunction : ValueFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public override string Name => "count";

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="nodeList">A nodelist</param>
	/// <returns>The number of nodes in the nodelist.</returns>
	public JsonNode? Evaluate(NodeList? nodeList)
	{
		return nodeList?.Count ?? 0;
	}
}