using System;
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
			new[] { ParameterType.Array | ParameterType.String }
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