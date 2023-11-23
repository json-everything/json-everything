using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class SliceSegment : IContextAccessorSegment
{
	private readonly int? _start;
	private readonly int? _end;
	private readonly int? _step;

	public SliceSegment(int? start, int? end, int? step)
	{
		_start = start;
		_end = end;
		_step = step;
	}

	public bool TryFind(JsonNode? contextValue, EvaluationContext fullContext, out JsonNode? value)
	{
		value = null;
		return contextValue switch
		{
			JsonArray arr => TryFind(arr, out value),
			JsonValue val when val.TryGetValue(out string? str) => TryFind(str, out value),
			_ => false
		};
	}

	private bool TryFind(JsonArray contextValue, out JsonNode? value)
	{
		if (_step == 0)
		{
			value = null;
			return false;
		}

		var result = new JsonArray();

		var step = _step ?? 1;
		var start = _start ?? (step >= 0 ? 0 : contextValue.Count);
		var end = _end ?? (step >= 0 ? contextValue.Count : -contextValue.Count - 1);
		var (lower, upper) = Bounds(start, end, step, contextValue.Count);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				result.Add(contextValue[i].Copy());
				i += step;
				if (i < 0) break; // overflow
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				result.Add(contextValue[i].Copy());
				i += step;
				if (i < 0) break; // overflow
			}
		}

		value = result;
		return true;
	}

	private bool TryFind(string contextValue, out JsonNode? value)
	{
		if (_step == 0)
		{
			value = null;
			return false;
		}

		var result = new StringBuilder();

		var step = _step ?? 1;
		var start = _start ?? (step >= 0 ? 0 : contextValue.Length);
		var end = _end ?? (step >= 0 ? contextValue.Length : -contextValue.Length - 1);
		var (lower, upper) = Bounds(start, end, step, contextValue.Length);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				result.Append(contextValue[i]);
				i += step;
				if (i < 0) break; // overflow
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				result.Append(contextValue[i]);
				i += step;
				if (i < 0) break; // overflow
			}
		}

		value = result.ToString();
		return true;
	}

	private static int Normalize(int i, int length)
	{
		return i >= 0 ? i : length + i;
	}

	private static (int lower, int upper) Bounds(int start, int end, int step, int length)
	{
		start = Normalize(start, length);
		end = Normalize(end, length);

		int lower, upper;

		if (step >= 0)
		{
			lower = Math.Min(Math.Max(start, 0), length);
			upper = Math.Min(Math.Max(end, 0), length);
		}
		else
		{
			upper = Math.Min(Math.Max(start, -1), length - 1);
			lower = Math.Min(Math.Max(end, -1), length - 1);
		}

		return (lower, upper);
	}
}