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
	public static KeywordConstraint? GetKeywordConstraint<T>(this IEnumerable<KeywordConstraint> constraints)
		where T : IJsonSchemaKeyword
	{
		return constraints.SingleOrDefault(x => x.Keyword == typeof(T).Keyword());
	}
}