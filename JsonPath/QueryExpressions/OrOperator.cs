using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class OrOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			if (left.OutputType == QueryExpressionType.Boolean) return QueryExpressionType.Boolean;
			return QueryExpressionType.Invalid;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			return (left.Value.GetBoolean() || right.Value.GetBoolean()).AsJsonElement();
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}||{right}";
		}
	}
}