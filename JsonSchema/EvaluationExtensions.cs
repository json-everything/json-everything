using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

public static class ConstraintExtensions
{
	public static KeywordEvaluation? GetKeywordEvaluation<T>(this KeywordEvaluation evaluation)
		where T : IJsonSchemaKeyword
	{
		return evaluation.SiblingEvaluations.SingleOrDefault(x => x.Constraint.Keyword == typeof(T).Keyword());
	}

	public static IEnumerable<KeywordEvaluation> GetNestedKeywordEvaluations<T>(this KeywordEvaluation evaluation)
		where T : IJsonSchemaKeyword
	{
		var local = GetKeywordEvaluation<T>(evaluation);
		if (local != null)
			yield return local;

		var candidateEvaluations = evaluation.SiblingEvaluations
			.SelectMany(x => x.ChildEvaluations.SelectMany(y => y.KeywordEvaluations));
		foreach (var child in candidateEvaluations)
		{
			if (child.Constraint.Keyword == typeof(T).Keyword()) yield return child;

			foreach (var grandchild in GetNestedKeywordEvaluations<T>(child))
			{
				yield return grandchild;
			}
		}
	}

	public static KeywordConstraint? GetKeywordConstraint<T>(this IEnumerable<KeywordConstraint> constraints)
		where T : IJsonSchemaKeyword
	{
		return constraints.SingleOrDefault(x => x.Keyword == typeof(T).Keyword());
	}
}