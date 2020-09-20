using System;
using System.Text.Json;

namespace Json.Path.Expressions
{
	internal class ExistsOperator : IQueryExpressionOperator
	{
		public QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right)
		{
			return QueryExpressionType.Boolean;
		}

		public JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right)
		{
			throw new NotImplementedException();
		}
	}
}