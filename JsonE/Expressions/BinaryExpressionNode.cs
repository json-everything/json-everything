using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class BinaryExpressionNode : ExpressionNode
{
	public IBinaryOperator Operator { get; }
	public ExpressionNode Left { get; }
	public ExpressionNode Right { get; set; }
	public int NestLevel { get; }

	public int Precedence => NestLevel * 10 + Operator.Precedence;

	public BinaryExpressionNode(IBinaryOperator op, ExpressionNode left, ExpressionNode right, int nestLevel)
	{
		Operator = op;
		Left = left;
		Right = right;
		NestLevel = nestLevel;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		return Operator.Evaluate(Left.Evaluate(context), Right.Evaluate(context));
	}

	public override void BuildString(StringBuilder builder)
	{
		var useGroup = Left is BinaryExpressionNode lBin &&
		               (lBin.Precedence - Precedence) % 10 > 1;

		if (useGroup)
			builder.Append('(');
		Left.BuildString(builder);
		if (useGroup)
			builder.Append(')');
	
		builder.Append(Operator);

		useGroup = Right is BinaryExpressionNode rBin &&
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
}
