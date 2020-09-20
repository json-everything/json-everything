using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class ModulusOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			if (left.OutputType == QueryExpressionType.Number) return QueryExpressionType.Number;
			return QueryExpressionType.Invalid;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			var rValue = right.Value.GetDecimal();
			if (rValue == 0) return default;
			return (left.Value.GetDecimal() % rValue).AsJsonElement();
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}%{right}";
		}
	}
}