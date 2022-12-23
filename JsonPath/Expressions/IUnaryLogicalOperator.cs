namespace Json.Path.Expressions;

internal interface IUnaryLogicalOperator : IExpressionOperator
{
	bool Evaluate(bool value);
}