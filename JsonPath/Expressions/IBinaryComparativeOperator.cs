namespace Json.Path.Expressions;

internal interface IBinaryComparativeOperator : IExpressionOperator
{
	bool Evaluate(PathValue? left, PathValue? right);
}