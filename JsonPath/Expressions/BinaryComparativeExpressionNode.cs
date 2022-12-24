using System.Diagnostics.CodeAnalysis;
using System;
using System.Text;
using System.Text.Json.Nodes;

namespace Json.Path.Expressions;

internal class BinaryComparativeExpressionNode : ComparativeExpressionNode
{
	public IBinaryComparativeOperator Operator { get; }
	public ValueExpressionNode Left { get; }
	public ValueExpressionNode Right { get; }

	public BinaryComparativeExpressionNode(IBinaryComparativeOperator op, ValueExpressionNode left, ValueExpressionNode right)
	{
		Operator = op;
		Left = left;
		Right = right;
	}

	public override bool Evaluate(JsonNode? globalParameter, JsonNode? localParameter)
	{
		return Operator.Evaluate(Left.Evaluate(globalParameter, localParameter), Right.Evaluate(globalParameter, localParameter));
	}

	public override void BuildString(StringBuilder builder)
	{
		Left.BuildString(builder);
		builder.Append(Operator);
		Right.BuildString(builder);
	}

	public override string ToString()
	{
		return $"{Left}{Operator}{Right}";
	}
}

internal class BinaryComparativeExpressionParser : IComparativeExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out ComparativeExpressionNode? expression)
	{
		var i = index;
		var nestLevel = 0;

		source.ConsumeWhitespace(ref i);
		while (i < source.Length && source[i] == '(')
		{
			nestLevel++;
			i++;
		}
		if (i == source.Length)
			throw new PathParseException(i, "Unexpected end of input");

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var left))
		{
			expression = null;
			return false;
		}

		// parse operator
		if (!BinaryComparativeOperatorParser.TryParse(source, ref i, out var op))
		{
			expression = null;
			return false;
		}

		// parse value
		if (!ValueExpressionParser.TryParse(source, ref i, out var right))
		{
			expression = null;
			return false;
		}

		source.ConsumeWhitespace(ref i);
		while (i < source.Length && source[i] == ')' && nestLevel > 0)
		{
			nestLevel--;
			i++;
		}
		if (i == source.Length)
			throw new PathParseException(i, "Unexpected end of input");
		if (nestLevel != 0)
		{
			expression = null;
			return false;
		}

		expression = new BinaryComparativeExpressionNode(op, left, right);
		index = i;
		return true;
	}
}