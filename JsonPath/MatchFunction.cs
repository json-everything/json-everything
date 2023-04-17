using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.Path;

/// <summary>
/// Implements the `match()` function which determines if a string exactly matches a regular
/// expression (using implicit anchoring).
/// </summary>
public class MatchFunction : LogicalFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public override string Name => "match";

	/// <summary>
	/// Evaluates the function.
	/// </summary>
	/// <param name="value">The value to test.</param>
	/// <param name="pattern">The iregexp pattern to test against.</param>
	/// <returns>true if the whole string is a match for the pattern; false otherwise.</returns>
	public bool Evaluate(JsonNode value, JsonNode pattern)
	{
		if (!value.TryGetValue<string>(out var text)) return false;
		if (!pattern.TryGetValue<string>(out var regex)) return false;

		var dotnetTranslation = regex.HandleDotNetSupportIssues();
		return Regex.IsMatch(text, $"^{dotnetTranslation}$", RegexOptions.ECMAScript);
	}
}