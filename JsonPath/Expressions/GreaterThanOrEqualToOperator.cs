using System;
using System.Text.Json;

namespace Json.Path.Expressions
{
	internal class GreaterThanOrEqualToOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputKind != right.OutputKind) return QueryExpressionType.Invalid;
			return left.OutputKind switch
			{
				QueryExpressionType.Number => QueryExpressionType.Boolean,
				QueryExpressionType.String => QueryExpressionType.Boolean,
				_ => QueryExpressionType.Invalid
			};
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			throw new NotImplementedException();
		}
	}
}