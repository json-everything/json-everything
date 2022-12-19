namespace Json.Path.Expressions;

internal interface IBinaryLogicalOperator
{
	bool Evaluate(bool left, bool right);
}