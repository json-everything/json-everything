using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions.Functions;

namespace Json.JsonE;

public class EvaluationContext
{
	private static readonly JsonObject _functionsContext =
		new()
		{
			["abs"] = new AbsFunction(),
			["ceil"] = new CeilFunction(),
			["defined"] = new DefinedFunction(),
			["floor"] = new FloorFunction(),
			["fromNow"] = new FromNowFunction(),
			["join"] = new JoinFunction(),
			["len"] = new LenFunction(),
			["lowercase"] = new LowercaseFunction(),
			["lstrip"] = new LStripFunction(),
			["max"] = new MaxFunction(),
			["min"] = new MinFunction(),
			["number"] = new NumberFunction(),
			["rstrip"] = new RStripFunction(),
			["split"] = new SplitFunction(),
			["sqrt"] = new SqrtFunction(),
			["str"] = new StrFunction(),
			["strip"] = new StripFunction(),
			["typeof"] = new TypeOfFunction(),
			["uppercase"] = new UppercaseFunction()
		};

	private readonly Stack<JsonObject> _contextStack;

	public EvaluationContext(JsonObject baseContext)
	{
		_contextStack = new Stack<JsonObject>();
		_contextStack.Push(_functionsContext);
		_contextStack.Push(new JsonObject { ["now"] = DateTime.Now.ToString("O") });
		_contextStack.Push(baseContext);
	}

	public void Push(JsonObject newContext)
	{
		_contextStack.Push(newContext);
	}

	public JsonNode? Pop()
	{
		return _contextStack.Pop();
	}

	public JsonNode? Find(ContextAccessor identifier)
	{
		foreach (var contextValue in _contextStack)
		{
			if (identifier.TryFind(contextValue, out var target)) return target;
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