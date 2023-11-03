namespace Json.JsonE.Expressions;

internal class OrOperator : IBinaryLogicalOperator
{
	public int Precedence => 21;

	public bool Evaluate(bool left, bool right)
	{
		return left || right;
	}

	public override string ToString()
	{
		return "||";
	}
}