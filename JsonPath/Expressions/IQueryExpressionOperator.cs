using System.Text.Json;

namespace Json.Path.Expressions
{
	internal interface IQueryExpressionOperator
	{
		QueryExpressionType GetOutputKind(QueryExpressionNode left, QueryExpressionNode right);
		JsonElement Evaluate(QueryExpressionNode left, QueryExpressionNode right);
	}
}