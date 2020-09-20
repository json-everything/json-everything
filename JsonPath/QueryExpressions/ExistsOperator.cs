using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class ExistsOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			return QueryExpressionType.Boolean;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			return (left.Value.ValueKind == JsonValueKind.Undefined).AsJsonElement();
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return left.ToString();
		}
	}
}