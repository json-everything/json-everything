using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE;

internal static class JsonNodeExtensions
{
	private static readonly JsonNode? _emptyString = string.Empty;
	private static readonly JsonNode? _zero = 0;
	private static readonly JsonNode? _false = false;

	public static bool IsTruthy(this JsonNode? node)
	{
		if (node is null) return false;
		if (node is JsonObject { Count: 0 }) return false;
		if (node is JsonArray { Count: 0 }) return false;
		if (node.IsEquivalentTo(_false)) return false;
		if (node.IsEquivalentTo(_zero)) return false;
		if (node.IsEquivalentTo(_emptyString)) return false;

		return true;
	}

	public static void ValidateAsContext(this JsonNode? context, string? location = null)
	{
		if (context is not JsonObject obj)
			throw new TemplateException($"{(location != null ? location + " value" : "context")} must be an object");

		if (obj.Any(x => !Regex.IsMatch(x.Key, "^[a-zA-Z_][a-zA-Z0-9_]*$")))
			throw new TemplateException($"top level keys of {location ?? "context"} must follow /[a-zA-Z_][a-zA-Z0-9_]*/");
	}

	public static bool IsTemplateOr<T>(this JsonNode? node)
	{
		return node switch
		{
			T => true,
			JsonObject when OperatorRepository.Get(node) != null => true,
			JsonValue value when value.TryGetValue<T>(out _) => true,
			_ => false
		};
	}

	public static void VerifyNoUndefinedProperties(this JsonObject obj, string op, params string[] additionalKeys)
	{
		var undefinedKeys = obj.Select(x => x.Key).Where(x => x != op && !additionalKeys.Contains(x)).ToArray();
		if (undefinedKeys.Length != 0)
			throw new TemplateException(CommonErrors.UndefinedProperties(op, undefinedKeys));
	}

	public static void VerifyNoUndefinedProperties(this JsonObject obj, string op, Regex additionalKey)
	{
		var undefinedKeys = obj.Select(x => x.Key).Where(x => x != op && !additionalKey.IsMatch(x)).ToArray();
		if (undefinedKeys.Length != 0)
			throw new TemplateException(CommonErrors.UndefinedProperties(op, undefinedKeys));
	}

	public static void ValidateNotReturningFunction(this JsonNode? result)
	{
		var queue = new Queue<JsonNode?>();
		queue.Enqueue(result);
		while (queue.Count != 0)
		{
			var current = queue.Dequeue();
			switch (current)
			{
				case JsonObject obj:
					foreach (var kvp in obj)
					{
						queue.Enqueue(kvp.Value);
					}
					break;
				case JsonArray arr:
					foreach (var item in arr)
					{
						queue.Enqueue(item);
					}
					break;
				case JsonValue val:
					if (val.TryGetValue<FunctionDefinition>(out _))
						throw new TemplateException("evaluated template contained uncalled functions");
					break;
			}
		}
	}

	public static JsonNode? Clone(this JsonNode? source)
	{
#if NET8_0_OR_GREATER
		JsonNode CopyObject(JsonObject obj)
		{
			var newObj = new JsonObject(obj.Options);
			foreach (var kvp in obj)
			{
				newObj[kvp.Key] = kvp.Value.Clone();
			}

			return newObj;
		}

		JsonNode CopyArray(JsonArray arr)
		{
			var newArr = new JsonArray(arr.Options);
			foreach (var item in arr)
			{
				newArr.Add(item.Clone());
			}

			return newArr;
		}

		JsonNode? CopyValue(JsonValue val)
		{
			if (val.TryGetValue(out FunctionDefinition? func))
				return JsonValue.Create(func, JsonESerializerContext.Default.FunctionDefinition);

			return val.DeepClone();
		}

		return source switch
		{
			null => null,
			JsonObject obj => CopyObject(obj),
			JsonArray arr => CopyArray(arr),
			JsonValue val => CopyValue(val),
			_ => throw new ArgumentOutOfRangeException(nameof(source))
		};
#else
		return source.Copy();
#endif
	}
}