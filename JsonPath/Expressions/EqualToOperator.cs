using Json.More;

namespace Json.Path.Expressions;

internal class EqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		if (left is null) return right is null;
		if (right is null) return false;

		if (!left.TryGetJson(out var lNode) ||
		    !right.TryGetJson(out var rNode))
			return false;

		return lNode.IsEquivalentTo(rNode);
	}

	public override string ToString()
	{
		return "==";
	}
}