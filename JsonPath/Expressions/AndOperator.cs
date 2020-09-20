using System;
using System.Text.Json;

namespace Json.Path.Expressions
{
	internal class AndOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputKind != right.OutputKind) return QueryExpressionType.Invalid;
			if (left.OutputKind == QueryExpressionType.Boolean) return QueryExpressionType.Boolean;
			return QueryExpressionType.Invalid;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			throw new NotImplementedException();
		}
	}
}