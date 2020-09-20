using System;
using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class LessThanOrEqualToOperator : IQueryExpressionOperator
	{
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

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			return left.OutputType switch
			{
				QueryExpressionType.Number => (left.Value.GetDecimal() <= right.Value.GetDecimal()).AsJsonElement(),
				QueryExpressionType.String => (string.Compare(left.Value.GetString(), right.Value.GetString(), StringComparison.Ordinal) <= 0).AsJsonElement(),
				_ => default
			};
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}<={right}";
		}
	}
}