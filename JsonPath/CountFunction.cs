using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Implements the `count()` function which returns the number of nodes
/// in a nodelist.
/// </summary>
public class CountFunction : IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public string Name => "length";

	/// <summary>
	/// The minimum argument count accepted by the function.
	/// </summary>
	public int MinArgumentCount => 1;

	/// <summary>
	/// The maximum argument count accepted by the function.
	/// </summary>
	public int MaxArgumentCount => 1;

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="arguments">A collection of nodelists where each nodelist in the collection corresponds to a single argument.</param>
	/// <returns>A nodelist.  If the evaluation fails, an empty nodelist is returned.</returns>
	public NodeList Evaluate(IEnumerable<NodeList> arguments)
	{
		return (JsonValue)arguments.First().Count;
	}
}