using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions.Functions;

namespace Json.JsonE;

/// <summary>
/// Provides context data for JSON-e evaluation.
/// </summary>
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

	internal EvaluationContext(JsonObject baseContext)
	{
		_contextStack = new Stack<JsonObject>();
		_contextStack.Push(_functionsContext);
		_contextStack.Push(new JsonObject { ["now"] = DateTime.Now.ToString("O") });
		_contextStack.Push(baseContext);
	}

	/// <summary>
	/// Adds or overrides context data.
	/// </summary>
	/// <param name="newContext"></param>
	public void Push(JsonObject newContext)
	{
		_contextStack.Push(newContext);
	}

	/// <summary>
	/// Removes the previous context data.  Call only if you've explicitly added context data with <see cref="Push(JsonObject)"/>
	/// </summary>
	/// <returns></returns>
	public JsonNode? Pop()
	{
		return _contextStack.Pop();
	}

	/// <summary>
	/// Finds data within the context.
	/// </summary>
	/// <param name="identifier">An accessor.</param>
	/// <returns>The value, if it exists.</returns>
	/// <exception cref="InterpreterException">Thrown if the context doesn't contain the indicated property.</exception>
	public JsonNode? Find(ContextAccessor identifier)
	{
		foreach (var contextValue in _contextStack)
		{
			if (identifier.TryFind(contextValue, out var target)) return target;
		}

		throw new InterpreterException($"unknown context value {identifier}");
	}

	/// <summary>
	/// Checks data for a given property.
	/// </summary>
	/// <param name="identifier">An accessor.</param>
	/// <returns>true if the value exists in the context; otherwise false.</returns>
	public bool IsDefined(ContextAccessor identifier)
	{
		foreach (var context in _contextStack)
		{
			if (identifier.TryFind(context, out _)) return true;
		}

		return false;
	}
}