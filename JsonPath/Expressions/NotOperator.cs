namespace Json.Path.Expressions;

internal class NotOperator : IUnaryLogicalOperator
{
	public bool Evaluate(bool value)
	{
		return !value;
	}
}