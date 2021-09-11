using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class NotOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 1;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			return QueryExpressionType.Boolean;
		}

		public JsonElementProxy Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			var lElement = left.Evaluate(element);
			return lElement.ValueKind == JsonValueKind.Undefined;
			//return lElement.ValueKind.In(JsonValueKind.False, JsonValueKind.Null, JsonValueKind.Undefined);
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			var lString = left.MaybeAddParentheses(OrderOfOperation);

			return $"!{lString}";
		}
	}
}