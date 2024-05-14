using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text;
using Json.JsonE.Operators;

namespace Json.JsonE.Expressions;

internal class FunctionExpressionNode : ExpressionNode
{
	public ExpressionNode FunctionExpression { get; }
	public ExpressionNode[] Parameters { get; }

	public FunctionExpressionNode(ExpressionNode functionExpression, IEnumerable<ExpressionNode> parameters)
	{
		FunctionExpression = functionExpression;
		Parameters = parameters.ToArray();
	}

	public override JsonNode? Evaluate(EvaluationContext context)
	{
		if (FunctionExpression.Evaluate(context) is not JsonValue functionNode)
			throw new InterpreterException($"unknown context value \"{FunctionExpression}\"");
		if (functionNode.TryGetValue(out string? functionName))
			functionNode = context.Find(functionName)!.AsValue();
		if (!functionNode.TryGetValue(out FunctionDefinition? function))
			throw new InterpreterException($"{functionNode} is not callable");

		var parameterValues = Parameters.Select(x => x.Evaluate(context)).ToArray();

		return function.Invoke(parameterValues, context);
	}

	public override void BuildString(StringBuilder builder)
	{
		//builder.Append(Function.Name);
		builder.Append('(');

		if (Parameters.Any())
		{
			Parameters[0].BuildString(builder);
			for (int i = 1; i < Parameters.Length; i++)
			{
				builder.Append(',');
				Parameters[i].BuildString(builder);
			}
		}

		builder.Append(')');
	}

	public override string ToString()
	{
		throw new NotImplementedException();
		//var parameterList = string.Join(", ", Parameters);
		//return $"{Function.Name}({parameterList})";
	}
}

internal static class FunctionArgumentParser
{
	public static bool TryParse(ReadOnlySpan<char> source, ref int index, out List<ExpressionNode>? arguments)
	{
		int i = index;

		if (!source.ConsumeWhitespace(ref i) || i == source.Length)
		{
			arguments = null;
			return false;
		}

		// consume (
		if (source[i] != '(')
		{
			arguments = null;
			return false;
		}

		i++;

		// parse list of arguments - all expressions
		arguments = [];
		var done = false;

		while (i < source.Length && !done)
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				arguments = null;
				return false;
			}

			if (source[i] == ')' && arguments.Count == 0)
			{
				i++;
				break;
			}

			if (!ExpressionParser.TryParse(source, ref i, out var expr))
				throw new SyntaxException(CommonErrors.WrongToken(source[i]));

			arguments.Add(expr!);

			if (!source.ConsumeWhitespace(ref i))
				throw new SyntaxException(CommonErrors.EndOfInput());

			switch (source[i])
			{
				case ')':
					done = true;
					break;
				case ',':
					break;
				default:
					arguments = null;
					return false;
			}

			i++;
		}

		index = i;
		return true;
	}
}