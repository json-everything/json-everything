using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class DivisionOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 2;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			if (left.OutputType == QueryExpressionType.Number) return QueryExpressionType.Number;
			return QueryExpressionType.Invalid;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			var rValue = right.Evaluate(element).GetDecimal();
			if (rValue == 0) return default;
			return (left.Evaluate(element).GetDecimal() / rValue).AsJsonElement();
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}/{right}";
		}
	}
}