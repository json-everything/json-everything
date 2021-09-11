using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class ExistsOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 1;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			return QueryExpressionType.Boolean;
		}

		public JsonElementProxy Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			return left.Evaluate(element).ValueKind != JsonValueKind.Undefined;
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return left.ToString();
		}
	}
}