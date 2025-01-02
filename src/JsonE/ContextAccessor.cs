using System;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions;
using Json.More;

namespace Json.JsonE;

/// <summary>
/// Represents a property.  Used for retrieving data from the context.
/// </summary>
public class ContextAccessor
{
	private readonly string _name;

	internal static ContextAccessor Now => "now";

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
			if (name!.In("true", "false", "null"))
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

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString() => _name;

	/// <summary>
	/// Implicitly converts a string into a <see cref="ContextAccessor"/>
	/// by using <see cref="TryParse(ReadOnlySpan{char}, ref int, out ContextAccessor?)"/>
	/// </summary>
	/// <param name="name"></param>
	public static implicit operator ContextAccessor(string name)
	{
		int index = 0;
		if (!TryParse(name.AsSpan(), ref index, out var accessor))
			throw new TemplateException($"{name} is not a valid accessor");

		return accessor!;
	}
}