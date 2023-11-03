namespace Json.JsonE.Expressions;

internal class NoOpOperator : IUnaryLogicalOperator
{
	public int Precedence => 20;

	public bool Evaluate(bool value)
	{
		return value;
	}

	public override string ToString()
	{
		return string.Empty;
	}
}