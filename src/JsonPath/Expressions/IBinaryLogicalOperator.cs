namespace Json.Path.Expressions;

internal interface IBinaryLogicalOperator : IExpressionOperator
{
	bool Evaluate(bool left, bool right);
}