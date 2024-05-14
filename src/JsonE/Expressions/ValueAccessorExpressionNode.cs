using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class ValueAccessorExpressionNode : ExpressionNode
{
	public ExpressionNode TargetExpression { get; }
	public ValueAccessor Accessor { get; }

	public ValueAccessorExpressionNode(ExpressionNode target, ValueAccessor accessor)
	{
		TargetExpression = target;
		Accessor = accessor;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		var target = TargetExpression.Evaluate(context);
		return Accessor.Find(target, context);
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Accessor);
	}

	public override string ToString()
	{
		return Accessor.ToString();
	}
}