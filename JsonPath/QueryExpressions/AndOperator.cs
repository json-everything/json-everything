using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class AndOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 5;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			if (left.OutputType == QueryExpressionType.Boolean) return QueryExpressionType.Boolean;
			return QueryExpressionType.Invalid;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			var lElement = left.Evaluate(element);
			if (!lElement.ValueKind.In(JsonValueKind.False, JsonValueKind.True)) return default;
			var rElement = right.Evaluate(element);
			if (!rElement.ValueKind.In(JsonValueKind.False, JsonValueKind.True)) return default;
			return (lElement.GetBoolean() && rElement.GetBoolean()).AsJsonElement();
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}&&{right}";
		}
	}
}