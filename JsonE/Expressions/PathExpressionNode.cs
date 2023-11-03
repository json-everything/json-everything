using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class PathExpressionNode : ValueExpressionNode
{
	public JsonPath Path { get; }

	public PathExpressionNode(JsonPath path)
	{
		Path = path;
	}

	public override PathValue? Evaluate(EvaluationContext context)
	{
		var result = Path.Evaluate(context);

		return result.Matches ?? NodeList.Empty;
	}

	public override void BuildString(StringBuilder builder)
	{
		Path.BuildString(builder);
	}

	public static implicit operator PathExpressionNode(JsonPath value)
	{
		return new PathExpressionNode(value);
	}

	public override string ToString()
	{
		return Path.ToString();
	}
}

internal class PathExpressionParser : IValueExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ValueExpressionNode? expression)
	{
		if (!PathParser.TryParse(source, ref index, out var path))
		{
			expression = null;
			return false;
		}

		expression = new PathExpressionNode(path);
		return true;
	}
}
