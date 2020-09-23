using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class EqualToOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 4;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			if (left.OutputType == QueryExpressionType.Invalid) return QueryExpressionType.Invalid;
			return QueryExpressionType.Boolean;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			return left.Evaluate(element).IsEquivalentTo(right.Evaluate(element)).AsJsonElement();
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}=={right}";
		}
	}
}