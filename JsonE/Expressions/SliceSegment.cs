using System;
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

	public bool TryFind(JsonNode? target, out JsonNode? value)
	{
		if (target is not JsonArray arr)
		{
			value = null;
			return false;
		}
		if (_step == 0)
		{
			value = null;
			return false;
		}

		var result = new JsonArray();

		var step = _step ?? 1;
		var start = _start ?? (step >= 0 ? 0 : arr.Count);
		var end = _end ?? (step >= 0 ? arr.Count : -arr.Count - 1);
		var (lower, upper) = Bounds(start, end, step, arr.Count);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				result.Add(arr[i].Copy());
				i += step;
				if (i < 0) break; // overflow
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				result.Add(arr[i].Copy());
				i += step;
				if (i < 0) break; // overflow
			}
		}

		value = result;
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