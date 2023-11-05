using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;

namespace Json.JsonE;

internal class EvaluationContext
{
	private readonly Stack<JsonNode?> _contextStack;

	public EvaluationContext(JsonNode? baseContext)
	{
		_contextStack = new Stack<JsonNode?>();
		_contextStack.Push(baseContext);
	}

	public void Push(JsonNode? newContext)
	{
		_contextStack.Push(newContext);
	}

	public JsonNode? Pop()
	{
		return _contextStack.Pop();
	}

	public JsonNode? Find(ContextAccessor identifier)
	{
		foreach (var context in _contextStack)
		{
			if (identifier.TryFind(context, out var target)) return target;
		}

		throw new InterpreterException($"unknown context value {identifier}");
	}

	public bool IsDefined(ContextAccessor identifier)
	{
		foreach (var context in _contextStack)
		{
			if (identifier.TryFind(context, out _)) return true;
		}

		return false;
	}
}