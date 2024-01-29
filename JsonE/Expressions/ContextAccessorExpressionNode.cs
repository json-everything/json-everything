using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class ContextAccessorExpressionNode : ExpressionNode
{
	public ContextAccessor Accessor { get; }

	public ContextAccessorExpressionNode(ContextAccessor accessor)
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

internal class ContextAccessorExpressionParser : IOperandExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
	{
		if (!ContextAccessor.TryParse(source, ref index, out var accessor))
		{
			expression = null;
			return false;
		}

		expression = new ContextAccessorExpressionNode(accessor!);
		return true;
	}
}