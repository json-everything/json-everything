using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class AccessorExpressionNode : ExpressionNode
{
	public ContextAccessor Accessor { get; }

	public AccessorExpressionNode(ContextAccessor accessor)
	{
		Accessor = accessor;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		return context.Find(Accessor);
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

internal class AccessorExpressionParser : IOperandExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
	{
		if (!ContextAccessor.TryParse(source, ref index, out var accessor))
		{
			expression = null;
			return false;
		}

		expression = new AccessorExpressionNode(accessor!);
		return true;
	}
}