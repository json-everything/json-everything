using System;
using System.Text.Json;

namespace Json.Path.Expressions
{
	internal class NotEqualToOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputKind != right.OutputKind) return QueryExpressionType.Invalid;
			if (left.OutputKind == QueryExpressionType.Invalid) return QueryExpressionType.Invalid;
			return QueryExpressionType.Boolean;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			throw new NotImplementedException();
		}
	}
}