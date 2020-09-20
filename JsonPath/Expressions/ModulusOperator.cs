using System;
using System.Text.Json;

namespace Json.Path.Expressions
{
	internal class ModulusOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right)
		{
			if (left.OutputKind != right.OutputKind) return QueryExpressionType.Invalid;
			if (left.OutputKind == QueryExpressionType.Number) return QueryExpressionType.Number;
			return QueryExpressionType.Invalid;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			throw new NotImplementedException();
		}
	}
}