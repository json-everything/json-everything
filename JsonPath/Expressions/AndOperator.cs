namespace Json.Path.Expressions;

internal class AndOperator : IBinaryLogicalOperator
{
	public bool Evaluate(bool left, bool right)
	{
		return left && right;
	}
}