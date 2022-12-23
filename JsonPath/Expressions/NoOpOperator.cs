namespace Json.Path.Expressions;

internal class NoOpOperator : IUnaryLogicalOperator
{
	public int Precedence => 20;

	public bool Evaluate(bool value)
	{
		return value;
	}
}