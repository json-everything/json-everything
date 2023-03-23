using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.Path;

/// <summary>
/// Implements the `match()` function which determines if any substring within
/// a string matches a regular expression.
/// </summary>
public class SearchFunction : LogicalFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public override string Name => "search";

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="value">The value to test.</param>
	/// <param name="pattern">The iregexp pattern to test against.</param>
	/// <returns>true if the string contains a match for the pattern; false otherwise.</returns>
	public bool Evaluate(JsonNode value, JsonNode pattern)
	{
		if (!value.TryGetValue<string>(out var text)) return false;
		if (!pattern.TryGetValue<string>(out var regex)) return false;

		return Regex.IsMatch(text, regex, RegexOptions.ECMAScript);
	}
}