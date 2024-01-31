using Json.More;

namespace Json.Path.Expressions;

internal class NotEqualToOperator : IBinaryComparativeOperator
{
	public int Precedence => 10;

	public bool Evaluate(PathValue? left, PathValue? right)
	{
		if (left is null) return right is not null;
		if (right is null) return true;

		if (!left.TryGetJson(out var lNode) ||
		    !right.TryGetJson(out var rNode))
			return true;

		return !lNode.IsEquivalentTo(rNode);
	}

	public override string ToString()
	{
		return "!=";
	}
}