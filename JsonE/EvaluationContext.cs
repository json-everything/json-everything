using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

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

	public JsonNode? Find(string identifier)
	{
		throw new NotImplementedException();
	}
}