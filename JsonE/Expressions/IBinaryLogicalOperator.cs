namespace Json.JsonE.Expressions;

internal interface IBinaryLogicalOperator : IExpressionOperator
{
	bool Evaluate(bool left, bool right);
}