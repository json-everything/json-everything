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

	public bool TryFind(JsonNode? contextValue, EvaluationContext fullContext, out JsonNode? value)
	{
		return TryFind(_index, contextValue, out value);
	}

	public static bool TryFind(int index, JsonNode? contextValue, out JsonNode? value)
	{
		if (contextValue is JsonArray arr)
		{
			if (index < 0)
			{
				if (-index < arr.Count)
				{
					value = arr[arr.Count + index];
					return true;
				}

				throw new InterpreterException("index out of bounds");
			}
			
			if (index < arr.Count)
			{
				value = arr[index];
				return true;
			}

			throw new InterpreterException("index out of bounds");
		}
		
		if (contextValue is JsonValue val && val.TryGetValue(out string? str))
		{
			if (index < 0)
			{
				if (-index < str.Length)
				{
					value = str[str.Length + index];
					return true;
				}
		
				throw new InterpreterException("index out of bounds");
			}
			
			if (index < str.Length)
			{
				value = str[index];
				return true;
			}
		
			throw new InterpreterException("index out of bounds");
		}

		if (contextValue is JsonObject)
			throw new InterpreterException("object keys must be strings");

		throw new InterpreterException("infix: \"[..]\" expects object, array, or string");
	}
}