namespace Json.Path.Expressions;

internal class ExistsOperator : IUnaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? value)
	{
		return value switch
		{
			JsonPathValue j => j.Value != null,
			NodeListPathValue n => n.Value.Count != 0,
			LogicalPathValue l => l.Value,
			_ => false
		};
	}

	public override string ToString()
	{
		return string.Empty;
	}
}