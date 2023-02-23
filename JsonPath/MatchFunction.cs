using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.Path;

/// <summary>
/// Implements the `match()` function which determines if a string exactly matches a regular
/// expression (using implicit anchoring).
/// </summary>
public class MatchFunction : IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public string Name => "match";

	/// <summary>
	/// Defines the sets of parameters that are valid for this function.
	/// </summary>
	/// <remarks>
	/// The value of this property is a collection of collections where
	/// each inner collection represents a single parameter set.  The
	/// outer collection represents differing parameter sets and can
	/// be thought of as "overloads."
	/// </remarks>
	public IEnumerable<IEnumerable<ParameterType>> ParameterSets { get; } =
		new[]
		{
			new[] { ParameterType.String, ParameterType.String }
		};

	/// <summary>
	/// The type returned by the function.
	/// </summary>
	/// <remarks>
	/// This is important for function composition: using a function
	/// as a parameter of another function.
	///
	/// This library assumes that a function may return `Nothing` and
	/// automatically handles that case.  This value should be set to
	/// what kind of non-`Nothing` type the function returns.
	///
	/// Registration of the function will throw an
	/// <see cref="InvalidOperationException"/> if the value is
	/// <see cref="FunctionType.Unspecified"/>
	/// </remarks>
	public FunctionType ReturnType => FunctionType.Boolean;

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="arguments">A collection of nodelists where each nodelist in the collection corresponds to a single argument.</param>
	/// <returns>A nodelist.  If the evaluation fails, an empty nodelist is returned.</returns>
	public NodeList Evaluate(IEnumerable<NodeList> arguments)
	{
		var args = arguments.ToArray();
		if (!args[0].TryGetSingleValue().TryGetValue<string>(out var text)) return NodeList.Empty;
		if (!args[1].TryGetSingleValue().TryGetValue<string>(out var regex)) return NodeList.Empty;

		return (JsonValue)Regex.IsMatch(text, $"^{regex}$", RegexOptions.ECMAScript);
	}
}