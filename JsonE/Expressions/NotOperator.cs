namespace Json.JsonE.Expressions;

internal class NotOperator : IUnaryLogicalOperator
{
	public int Precedence => 20;

	public bool Evaluate(bool value)
	{
		return !value;
	}

	public override string ToString()
	{
		return "!";
	}
}