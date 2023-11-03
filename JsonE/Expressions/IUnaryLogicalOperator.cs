namespace Json.JsonE.Expressions;

internal interface IUnaryLogicalOperator : IExpressionOperator
{
	bool Evaluate(bool value);
}