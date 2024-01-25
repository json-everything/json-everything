using System;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Expressions;

internal class SliceSegment : IContextAccessorSegment
{
	private readonly ExpressionNode? _start;
	private readonly ExpressionNode? _end;
	private readonly ExpressionNode? _step;

	public SliceSegment(ExpressionNode? start, ExpressionNode? end, ExpressionNode? step)
	{
		_start = start;
		_end = end;
		_step = step;
	}

	public bool TryFind(JsonNode? contextValue, EvaluationContext fullContext, out JsonNode? value)
	{
		var (start, end, step) = GetValues(fullContext);
		value = null;
		return contextValue switch
		{
			JsonArray arr => TryFind(start, end, step, arr, out value),
			JsonValue val when val.TryGetValue(out string? str) => TryFind(start, end, step, str, out value),
			_ => false
		};
	}

	private (int?, int?, int?) GetValues(EvaluationContext context)
	{
		return (
			GetValue(_start, context),
			GetValue(_end, context),
			GetValue(_step, context)
		);
	}

	private static int? GetValue(ExpressionNode? expr, EvaluationContext context)
	{
		if (expr is null) return null;

		var node = expr.Evaluate(context);
		if (node is not JsonValue value)
			throw new InterpreterException("cannot perform interval access with non-integers");
		var num = value.GetNumber();
		var index = (int?)num;
		if (num == null || num != index)
			throw new InterpreterException("cannot perform interval access with non-integers");

		return index;
	}

	private static bool TryFind(int? start, int? end, int? step, JsonArray contextValue, out JsonNode? value)
	{
		if (step == 0)
		{
			value = null;
			return false;
		}

		var result = new JsonArray();

		step ??= 1;
		start ??= (step >= 0 ? 0 : contextValue.Count);
		end ??= (step >= 0 ? contextValue.Count : -contextValue.Count - 1);
		var (lower, upper) = Bounds(start.Value, end.Value, step.Value, contextValue.Count);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				result.Add(contextValue[i].Copy());
				i += step.Value;
				if (i < 0) break; // overflow
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				result.Add(contextValue[i].Copy());
				i += step.Value;
				if (i < 0) break; // overflow
			}
		}

		value = result;
		return true;
	}

	private static bool TryFind(int? start, int? end, int? step, string contextValue, out JsonNode? value)
	{
		if (step == 0)
		{
			value = null;
			return false;
		}

		var result = new StringBuilder();
		var stringInfo = new StringInfo(contextValue);

		step ??= 1;
		start ??= (step >= 0 ? 0 : stringInfo.LengthInTextElements);
		end ??= (step >= 0 ? stringInfo.LengthInTextElements : -stringInfo.LengthInTextElements - 1);
		var (lower, upper) = Bounds(start.Value, end.Value, step.Value, stringInfo.LengthInTextElements);

		if (step > 0)
		{
			var i = lower;
			while (i < upper)
			{
				result.Append(stringInfo.SubstringByTextElements(i, 1));
				i += step.Value;
				if (i < 0) break; // overflow
			}
		}
		else
		{
			var i = upper;
			while (lower < i)
			{
				result.Append(stringInfo.SubstringByTextElements(i, 1));
				i += step.Value;
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