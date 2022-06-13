using System.Text.Json.Nodes;

namespace Json.Logic;

/// <summary>
/// Calls <see cref="Rule.Apply"/> with no data.
/// </summary>
public static class RuleExtensions
{
	/// <summary>
	/// Calls <see cref="Rule.Apply"/> with no data.
	/// </summary>
	/// <param name="rule">The rule.</param>
	/// <returns>The result.</returns>
	public static JsonNode? Apply(this Rule rule)
	{
		return rule.Apply(null);
	}
}