using System;
using System.Text.Json;

namespace Json.Path.Expressions
{
	internal class AdditionOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputKind != right.OutputKind) return QueryExpressionType.Invalid;
			return left.OutputKind switch
			{
				QueryExpressionType.Number => QueryExpressionType.Number,
				QueryExpressionType.String => QueryExpressionType.String,
				_ => QueryExpressionType.Invalid
			};
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			throw new NotImplementedException();
		}
	}
}