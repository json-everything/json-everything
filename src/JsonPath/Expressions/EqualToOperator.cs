using Json.More;

namespace Json.Path.Expressions;

internal class EqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		if (left is null) return right is null;
		if (right is null) return false;

		var lSuccess = left.TryGetJson(out var lNode) && !ReferenceEquals(lNode, ValueFunctionDefinition.Nothing);
		var rSuccess = right.TryGetJson(out var rNode) && !ReferenceEquals(rNode, ValueFunctionDefinition.Nothing);

		if (!lSuccess && !rSuccess) return true;
		if (!lSuccess || !rSuccess) return false;

		return lNode.IsEquivalentTo(rNode);
	}

	public override string ToString()
	{
		return "==";
	}
}