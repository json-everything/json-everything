using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryLogicalExpressionNode : CompositeLogicalExpressionNode
{
	public LogicalExpressionNode Left { get; set; }
	public IBinaryLogicalOperator Operator { get; }
	public LogicalExpressionNode Right { get; set; }
	public int NestLevel { get; }

	public int Precedence => NestLevel * 10 + Operator.Precedence;

	public BinaryLogicalExpressionNode(LogicalExpressionNode left, IBinaryLogicalOperator op, LogicalExpressionNode right, int nestLevel)
	{
		Operator = op;
		Left = left;
		Right = right;
		NestLevel = nestLevel;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		var left = Left.Evaluate(globalParameter, localParameter);
		var right = Right.Evaluate(globalParameter, localParameter);
		var result = Operator.Evaluate(left, right);
		return result;
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Left is BinaryLogicalExpressionNode lBin && lBin.Precedence > Operator.Precedence;

		if (useGroup)
			builder.Append('(');
		Left.BuildString(builder);
		if (useGroup)
			builder.Append(')');
		builder.Append(Operator);
		
		useGroup = Right is BinaryLogicalExpressionNode rBin && rBin.Precedence > Operator.Precedence;

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
}
