using Json.More;

namespace Json.Path.Expressions;

internal class EqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		var lValue = left?.TryGetJson();
		var rValue = right?.TryGetJson();

		return lValue.IsEquivalentTo(rValue);
	}

	public override string ToString()
	{
		return "==";
	}
}