using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class AdditionOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 3;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			return left.OutputType switch
			{
				QueryExpressionType.Number => QueryExpressionType.Number,
				QueryExpressionType.String => QueryExpressionType.String,
				_ => QueryExpressionType.Invalid
			};
		}

		public JsonElementProxy Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			var lElement = left.Evaluate(element);
			var rElement = right.Evaluate(element);
			switch (left.OutputType)
			{
				case QueryExpressionType.Number:
					if (lElement.ValueKind != JsonValueKind.Number ||
					    rElement.ValueKind != JsonValueKind.Number) return default;
					return lElement.GetDecimal() + rElement.GetDecimal();
				case QueryExpressionType.String:
					if (lElement.ValueKind != JsonValueKind.String ||
					    rElement.ValueKind != JsonValueKind.String) return default;
					return string.Concat(lElement.GetString(), rElement.GetString());
				default:
					return default;
			}
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			var lString = left.MaybeAddParentheses(OrderOfOperation);
			var rString = right.MaybeAddParentheses(OrderOfOperation);

			return $"{lString}+{rString}";
		}
	}
}