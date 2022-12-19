namespace Json.Path.Expressions;

internal class OrOperator : IBinaryLogicalOperator
{
	public bool Evaluate(bool left, bool right)
	{
		return left || right;
	}
}