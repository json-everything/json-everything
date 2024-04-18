using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic;

/// <summary>
/// Provides context data for JSON-e evaluation.
/// </summary>
public class EvaluationContext
{
	private readonly Stack<JsonNode?> _contextStack;

	/// <summary>
	/// The top-level context value.
	/// </summary>
	public JsonNode? CurrentValue => _contextStack.Peek();

	internal EvaluationContext(JsonNode? baseContext)
	{
		_contextStack = new Stack<JsonNode?>();
		_contextStack.Push(baseContext);
	}

	/// <summary>
	/// Adds or overrides context data.
	/// </summary>
	/// <param name="newContext"></param>
	public void Push(JsonNode? newContext)
	{
		_contextStack.Push(newContext);
	}

	/// <summary>
	/// Removes the previous context data.  Call only if you've explicitly added context data with <see cref="Push(JsonNode)"/>
	/// </summary>
	/// <returns></returns>
	public JsonNode? Pop()
	{
		return _contextStack.Pop();
	}

	/// <summary>
	/// Attempts to resolve a variable path within the entire context.
	/// </summary>
	/// <param name="path">The variable path.</param>
	/// <param name="result">The result, if found; null otherwise..</param>
	/// <returns>true if the path was found; false otherwise.</returns>
	public bool TryFind(string? path, out JsonNode? result)
	{
		if (path == null)
		{
			result = null;
			return false;
		}

		var parts = path.Split('.');
		foreach (var node in _contextStack)
		{
			if (TryFindInSingleLayer(node, parts, out result)) return true;
		}

		result = null;
		return false;
	}

	private static bool TryFindInSingleLayer(JsonNode? layer, string[] parts, out JsonNode? result)
	{
		var current = layer;
		foreach (var part in parts)
		{
			switch (current)
			{
				case JsonObject obj:
					if (!obj.TryGetValue(part, out current, out _))
					{
						result = null;
						return false;
					}
					break;
				case JsonArray array:
					if (!int.TryParse(part, out var index))
					{
						result = null;
						return false;
					}
					if (0 >= array.Count || array.Count <= index)
					{
						result = null;
						return false;
					}
					current = array[index];
					break;
				default:
					result = null;
					return false;
			}
		}

		result = current;
		return true;
	}
}