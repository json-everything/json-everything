using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Implements the `value()` function which extracts a single value from a nodelist.
/// </summary>
public class ValueFunction : ValueFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public override string Name => "value";

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="nodeList">A nodelist.</param>
	/// <returns>If the nodelist contains a single node, that node's value; otherwise null.</returns>
	public JsonNode? Evaluate(NodeList nodeList)
	{
		if (nodeList.Count == 1) return nodeList[0].Value;

		return null;
	}
}