using System.Collections.Generic;

namespace Json.Path;

/// <summary>
/// Defines properties and methods required for an expression function.
/// </summary>
/// <remarks>Functions must be registered with <see cref="FunctionRepository.Register(IPathFunctionDefinition)"/></remarks>
public interface IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	string Name { get; }
	/// <summary>
	/// The minimum argument count accepted by the function.
	/// </summary>
	int MinArgumentCount { get; }
	/// <summary>
	/// The maximum argument count accepted by the function.
	/// </summary>
	int MaxArgumentCount { get; }

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="arguments">A collection of nodelists where each nodelist in the collection corresponds to a single argument.</param>
	/// <returns>A nodelist.  If the evaluation fails, an empty nodelist is returned.</returns>
	NodeList Evaluate(IEnumerable<NodeList> arguments);
}