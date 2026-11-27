using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class CompositeValueExpressionNode : ValueExpressionNode
{
	public ValueExpressionNode Left { get; }
	public IBinaryValueOperator Operator { get; }
	public ValueExpressionNode Right { get; set; }
	public int NestLevel { get; }

	public int Precedence => NestLevel * 10 + Operator.Precedence;

	public CompositeValueExpressionNode(ValueExpressionNode left, IBinaryValueOperator op, ValueExpressionNode right, int nestLevel)
	{
		Operator = op;
		Left = left;
		Right = right;
		NestLevel = nestLevel;
	}

	public override PathValue? Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Left is CompositeValueExpressionNode lBin &&
		               (lBin.Precedence - Precedence) % 10 > 1;

		if (useGroup)
			builder.Append('(');
		Left.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	
		builder.Append(Operator);

		useGroup = Right is CompositeValueExpressionNode rBin &&
		           (rBin.Precedence - Precedence) % 10 > 1;
		if (useGroup)
			builder.Append('(');
		Right.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	}

	public override string ToString()
	{
		return $"{Left}{Operator}{Right}";
	}

	public override bool Equals(object? obj) =>
		obj is CompositeValueExpressionNode other &&
		Equals(Operator, other.Operator) &&
		Equals(Left, other.Left) &&
		Equals(Right, other.Right);

	public override int GetHashCode()
	{
		unchecked
		{
			var hash = Operator.GetHashCode();
			hash = (hash * 397) ^ Left.GetHashCode();
			hash = (hash * 397) ^ Right.GetHashCode();
			return hash;
		}
	}
}
