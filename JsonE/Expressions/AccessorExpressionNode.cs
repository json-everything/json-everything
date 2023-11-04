using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class AccessorExpressionNode : ValueExpressionNode
{
	public string Accessor { get; }

	public AccessorExpressionNode(string accessor)
	{
		Accessor = accessor;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		throw new NotImplementedException();
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Accessor);
	}

	public static implicit operator AccessorExpressionNode(string value)
	{
		return new AccessorExpressionNode(value);
	}

	public override string ToString()
	{
		return Accessor;
	}
}

internal class AccessorExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ValueExpressionNode? expression)
	{
		throw new NotImplementedException();
		//if (!PathParser.TryParse(source, ref index, out var path))
		//{
		//	expression = null;
		//	return false;
		//}

		//expression = new AccessorExpressionNode(path);
		return true;
	}
}
