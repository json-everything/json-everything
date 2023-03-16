using System.Linq;

namespace Json.Path.Expressions;

internal class ExistsOperator : IUnaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? value)
	{
		return value switch
		{
			JsonPathValue j => j.Value != null,
			NodeListPathValue n => n.Value.Any(),
			LogicalPathValue l => l.Value,
			_ => false
		};
	}

	public override string ToString()
	{
		return string.Empty;
	}
}