using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
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
		var done = false;

		while (i < source.Length)
		{
			if (!source.ConsumeWhitespace(ref i))
				throw new SyntaxException(CommonErrors.EndOfInput());

			// read name
			if (!TryParseString(source, ref i, out var key))
			{
				if (source[i] is '}')
				{
					done = true;
					i++;
					break;
				}

				expression = null;
				return false;
			}

			if (!source.ConsumeWhitespace(ref i))
				throw new SyntaxException(CommonErrors.EndOfInput());

			// read :
			if (source[i] != ':')
			{
				expression = null;
				return false;
			}

			i++;

			if (!source.ConsumeWhitespace(ref i))
				throw new SyntaxException(CommonErrors.EndOfInput());

			// read expression
			if (!ExpressionParser.TryParse(source, ref i, out var value))
				throw new SyntaxException(CommonErrors.WrongToken(source[i]));

			obj[key!] = JsonExpression.Create(value!);

			// read , or }
			if (source[i] is ',')
			{
				i++;
				continue;
			}
			if (source[i] is '}')
			{
				done = true;
				i++;
				break;
			}

			throw new SyntaxException(CommonErrors.WrongToken(source[i]));
		}

		if (!done) throw new SyntaxException(CommonErrors.EndOfInput());

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