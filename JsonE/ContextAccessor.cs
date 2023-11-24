using System;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.More;

namespace Json.JsonE;

public class ContextAccessor
{
	private readonly string _name;

	internal static ContextAccessor Now { get; } = "now";
	internal static ContextAccessor Default { get; } = "x";

	private ContextAccessor(string asString)
	{
		_name = asString;
	}

	internal static bool TryParse(ReadOnlySpan<char> source, ref int index, out ContextAccessor? accessor)
	{
		var i = index;
		if (!source.ConsumeWhitespace(ref i))
		{
			accessor = null;
			return false;
		}

		if (source.TryParseName(ref i, out var name))
		{
			if (name.In("true", "false", "null"))
			{
				accessor = null;
				return false;
			}
		}
		else
		{
			accessor = null;
			return false;
		}

		index = i;
		accessor = new ContextAccessor(name!);
		return true;
	}

	internal bool TryFind(JsonObject context, out JsonNode? value)
	{
		return context.TryGetValue(_name, out value, out _);
	}

	public override string ToString() => _name;

	public static implicit operator ContextAccessor(string name)
	{
		return new ContextAccessor(name);
	}
}