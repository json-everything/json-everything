using Json.More;

namespace Json.Path.Expressions;

internal class NotEqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		if (left is null) return right is not null;
		if (right is null) return true;

		var lSuccess = left.TryGetJson(out var lNode) && !ReferenceEquals(lNode, ValueFunctionDefinition.Nothing);
		var rSuccess = right.TryGetJson(out var rNode) && !ReferenceEquals(rNode, ValueFunctionDefinition.Nothing);

		if (!lSuccess && !rSuccess) return false;
		if (!lSuccess || !rSuccess) return true;

		return !lNode.IsEquivalentTo(rNode);
	}

	public override string ToString()
	{
		return "!=";
	}
}