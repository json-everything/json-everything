using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class ModulusOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 2;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			if (left.OutputType == QueryExpressionType.Number) return QueryExpressionType.Number;
			return QueryExpressionType.Invalid;
		}

		public JsonElementProxy Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			var rElement = right.Evaluate(element);
			if (rElement.ValueKind != JsonValueKind.Number) return default;
			var rValue = rElement.GetDecimal();
			if (rValue == 0) return default;
			var lElement = left.Evaluate(element);
			if (lElement.ValueKind != JsonValueKind.Number) return default;
			return lElement.GetDecimal() % rValue;
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			var lString = left.MaybeAddParentheses(OrderOfOperation);
			var rString = right.MaybeAddParentheses(OrderOfOperation, true);

			return $"{lString}%{rString}";
		}
	}
}