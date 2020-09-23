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

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right, JsonElement element)
		{
			return left.OutputType switch
			{
				QueryExpressionType.Number => (left.Evaluate(element).GetDecimal() + right.Evaluate(element).GetDecimal()).AsJsonElement(),
				QueryExpressionType.String => string.Concat(left.Evaluate(element).GetString(), right.Evaluate(element).GetString()).AsJsonElement(),
				_ => default
			};
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}+{right}";
		}
	}
}