namespace Json.JsonE.Expressions;

internal class AndOperator : IBinaryLogicalOperator
{
	public int Precedence => 22;

	public bool Evaluate(bool left, bool right)
	{
		return left && right;
	}

	public override string ToString()
	{
		return "&&";
	}
}