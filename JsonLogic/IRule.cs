using System.Text.Json.Nodes;

namespace Json.Logic;

/// <summary>
/// Defines functionality for a model-less approach to evaluating JSON Logic rules.
/// </summary>
public interface IRule
{
	/// <summary>
	/// Applies the rule.
	/// </summary>
	/// <param name="args">The arguments.</param>
	/// <param name="context">The context.</param>
	/// <returns>The result.</returns>
	JsonNode? Apply(JsonNode? args, EvaluationContext context);
}