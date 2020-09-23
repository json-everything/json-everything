using System;
using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class GreaterThanOperator : IQueryExpressionOperator
	{
		public int OrderOfOperation => 4;

		public QueryExpressionType GetOutputType(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputType != right.OutputType) return QueryExpressionType.Invalid;
			return left.OutputType switch
			{
				QueryExpressionType.Number => QueryExpressionType.Boolean,
				QueryExpressionType.String => QueryExpressionType.Boolean,
				_ => QueryExpressionType.Invalid
			};
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			var leftValue = left.Evaluate(element);
			var rightValue = right.Evaluate(element);

			if (leftValue.ValueKind != rightValue.ValueKind) return default;

			return leftValue.ValueKind switch
			{
				JsonValueKind.Number => (leftValue.GetDecimal() > rightValue.GetDecimal()).AsJsonElement(),
				JsonValueKind.String => (string.Compare(leftValue.GetString(), rightValue.GetString(), StringComparison.Ordinal) > 0).AsJsonElement(),
				_ => default
			};
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}>{right}";
		}
	}
}