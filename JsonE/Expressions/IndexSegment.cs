using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class IndexSegment : IContextAccessorSegment
{
	private readonly int _index;

	public IndexSegment(int index)
	{
		_index = index;
	}

	public bool TryFind(JsonNode? contextValue, out JsonNode? value)
	{
		if (contextValue is JsonArray arr)
		{
			if (_index < 0)
			{
				if (-_index < arr.Count)
				{
					value = arr[arr.Count + _index];
					return true;
				}

				throw new InterpreterException("index out of bounds");
			}
			
			if (_index < arr.Count)
			{
				value = arr[_index];
				return true;
			}

			throw new InterpreterException("index out of bounds");
		}
		
		if (contextValue is JsonValue val && val.TryGetValue(out string? str))
		{
			if (_index < 0)
			{
				if (-_index < str.Length)
				{
					value = str[str.Length + _index];
					return true;
				}
		
				throw new InterpreterException("index out of bounds");
			}
			
			if (_index < str.Length)
			{
				value = str[_index];
				return true;
			}
		
			throw new InterpreterException("index out of bounds");
		}

		if (contextValue is JsonObject)
			throw new InterpreterException("object keys must be strings");

		throw new InterpreterException("infix: \"[..]\" expects object, array, or string");
	}
}

internal class ExpressionSegment : IContextAccessorSegment
{
	private readonly ExpressionNode _expression;

	public ExpressionSegment(ExpressionNode expression)
	{
		_expression = expression;
	}

	public bool TryFind(JsonNode? contextValue, out JsonNode? value)
	{
		throw new NotImplementedException();
	}
}