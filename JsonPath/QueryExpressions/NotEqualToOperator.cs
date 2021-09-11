using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class NotEqualToOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 4;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType == QueryExpressionType.Invalid ||
			    right.OutputType == QueryExpressionType.Invalid)
				return QueryExpressionType.Invalid;
			return QueryExpressionType.Boolean;
		}

		public JsonElementProxy Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			return !left.Evaluate(element).IsEquivalentTo(right.Evaluate(element));
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			var lString = left.MaybeAddParentheses(OrderOfOperation);
			var rString = right.MaybeAddParentheses(OrderOfOperation);

			return $"{lString}!={rString}";
		}
	}
}