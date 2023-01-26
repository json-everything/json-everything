using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Implements the `length()` function to get:
/// - the length of a string
/// - the count of values in an array
/// - the count of values in an object
/// </summary>
public class LengthFunction : IPathFunctionDefinition
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

	public IEnumerable<IEnumerable<ParameterType>> ParameterSets { get; } =
		new[]
		{
			new[] { ParameterType.Array | ParameterType.String }
		};

	public FunctionType ReturnType => FunctionType.Value;

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="arguments">A collection of nodelists where each nodelist in the collection corresponds to a single argument.</param>
	/// <returns>A nodelist.  If the evaluation fails, an empty nodelist is returned.</returns>
	public NodeList Evaluate(IEnumerable<NodeList> arguments)
	{
		var node = arguments.Single().TryGetSingleValue();

		return node switch
		{
			JsonObject obj => (JsonValue)obj.Count,
			JsonArray arr => (JsonValue)arr.Count,
			JsonValue val when val.TryGetValue(out string? s) => (JsonValue)s.Length,
			JsonValue val when val.TryGetValue(out NodeList? l) => (JsonValue)l.Count,
			_ => NodeList.Empty
		};
	}
}