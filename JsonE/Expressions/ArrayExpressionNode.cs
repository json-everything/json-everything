using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions;

internal class ArrayExpressionNode : ExpressionNode
{
	public JsonArray Value { get; }

	public ArrayExpressionNode(JsonArray value)
	{
		Value = value;
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		return JsonE.Evaluate(Value, context);
	}

	public override void BuildString(StringBuilder builder)
	{
		builder.Append(Value.AsJsonString());
	}

	public override string ToString()
	{
		return Value.AsJsonString();
	}
}

internal class ArrayExpressionParser : IOperandExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
	{
		if (source[index] != '[')
		{
			expression = null;
			return false;
		}

		// consume {
		int i = index + 1;
		var arr = new JsonArray();

		while (i < source.Length && source[i] != ']')
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}

			// read expression
			if (!ExpressionParser.TryParse(source, ref i, out var value))
				throw new SyntaxException(CommonErrors.WrongToken(source[i]));

			arr.Add((JsonNode)(JsonValue)JsonExpression.Create(value!));

			// read , or ]
			if (source[i] is ',')
			{
				i++;
				continue;
			}
			if (source[i] is ']')
			{
				i++;
				break;
			}

			expression = null;
			return false;
		}

		if (arr.Count == 0 && source[i] == ']')
			i++;

		index = i;
		expression = new ArrayExpressionNode(arr);
		return true;
	}
}