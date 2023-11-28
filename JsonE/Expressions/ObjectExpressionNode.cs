using System;
using System.Text;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class ObjectExpressionNode : ExpressionNode
{
	public JsonObject Value { get; }

	public ObjectExpressionNode(JsonObject value)
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

internal class ObjectExpressionParser : IOperandExpressionParser
{
	public bool TryParse(ReadOnlySpan<char> source, ref int index, out ExpressionNode? expression)
	{
		if (source[index] != '{')
		{
			expression = null;
			return false;
		}

		// consume {
		int i = index + 1;
		var obj = new JsonObject();

		while (i < source.Length && source[i] != '}')
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}

			// read name
			if (!TryParseString(source, ref i, out var key))
			{
				expression = null;
				return false;
			}

			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}

			// read :
			if (source[i] != ':')
			{
				expression = null;
				return false;
			}

			i++;

			if (!source.ConsumeWhitespace(ref i))
			{
				expression = null;
				return false;
			}

			// read expression
			if (!ExpressionParser.TryParse(source, ref i, out var value))
			{
				expression = null;
				return false;
			}

			obj[key!] = JsonExpression.Create(value!);

			// read , or }
			if (source[i] is ',')
			{
				i++;
				if (source[i] is '}')
				{
					expression = null;
					return false;
				}
				continue;
			}
			if (source[i] is '}')
			{
				i++;
				break;
			}

			expression = null;
			return false;
		}

		if (obj.Count == 0 && source[i] == '}')
			i++;

		index = i;
		expression = new ObjectExpressionNode(obj);
		return true;
	}

	private static bool TryParseString(ReadOnlySpan<char> source, ref int index, out string? str)
	{
		var i = index;
		char? quoteChar = source[i] is ('\'' or '"') ? source[i] : null;

		if (!quoteChar.HasValue) return source.TryParseName(ref index, out str);

		i++;
		while (i < source.Length && source[i] != quoteChar)
		{
			i++;
		}

		if (i == source.Length)
		{
			str = null;
			return false;
		}

		str = source[(index + 1)..i].ToString();
		index = i + 1; // consume quote
		return true;

	}
}