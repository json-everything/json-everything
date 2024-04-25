using System;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Convenience extensions for building constraints and processing evaluations.
/// </summary>
public static class ConstraintExtensions
{
	/// <summary>
	/// Gets an evaluation from a keyword with the given type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	/// <param name="evaluation">The source evaluation.</param>
	/// <returns>The keyword evaluation, if it exists.</returns>
	public static KeywordEvaluation? GetKeywordEvaluation<T>(this KeywordEvaluation evaluation)
		where T : IJsonSchemaKeyword
	{
		return evaluation.SiblingEvaluations.SingleOrDefault(x => x.Constraint.Keyword == typeof(T).Keyword());
	}

	/// <summary>
	/// Gets a constraint from a keyword with the given type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	/// <param name="constraints">The set of constraints.</param>
	/// <returns>The keyword evaluation, if it exists.</returns>
	public static KeywordConstraint? GetKeywordConstraint<T>(this ReadOnlySpan<KeywordConstraint> constraints)
		where T : IJsonSchemaKeyword
	{
		foreach (var constraint in constraints)
		{
			if (constraint.Keyword == typeof(T).Keyword()) return constraint;
		}
		return null;
	}
}