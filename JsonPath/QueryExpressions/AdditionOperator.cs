using System.Text.Json;
using Json.More;

namespace Json.Path.QueryExpressions
{
	internal class AdditionOperator : IQueryExpressionOperator
	{
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

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			return left.OutputType switch
			{
				QueryExpressionType.Number => (left.Value.GetDecimal() + right.Value.GetDecimal()).AsJsonElement(),
				QueryExpressionType.String => string.Concat(left.Value.GetString(), right.Value.GetString()).AsJsonElement(),
				_ => default
			};
		}

		public string ToString(QueryExpressionNode left, QueryExpressionNode right)
		{
			return $"{left}+{right}";
		}
	}
}