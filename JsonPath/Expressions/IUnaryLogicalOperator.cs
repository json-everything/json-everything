namespace Json.Path.Expressions;

internal interface IUnaryLogicalOperator
{
	bool Evaluate(bool value);
}