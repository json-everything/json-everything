namespace Json.Path.Expressions;

internal interface IUnaryComparativeOperator : IExpressionOperator
{
	bool Evaluate(PathValue? value);
}