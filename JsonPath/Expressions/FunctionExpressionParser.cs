using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Path.Expressions;

internal static class FunctionExpressionParser
{
	private class EqualOrUnspecifiedEqualityComparer : IEqualityComparer<ParameterType>
	{
		public static EqualOrUnspecifiedEqualityComparer Instance { get; } = new();

		private EqualOrUnspecifiedEqualityComparer(){}

		public bool Equals(ParameterType x, ParameterType y)
		{
			return x == ParameterType.Unspecified || y == ParameterType.Unspecified || x == y;
		}

		public int GetHashCode(ParameterType obj)
		{
			return 0;
		}
	}

	public static bool TryParseFunction(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out List<ValueExpressionNode>? parameters, [NotNullWhen(true)] out IPathFunctionDefinition? function, PathParsingOptions options)
	{
		int i = index;

		if (!source.ConsumeWhitespace(ref i))
		{
			parameters = null;
			function = null;
			return false;
		}

		// parse function name
		if (!source.TryParseName(ref i, out var name))
		{
			parameters = null;
			function = null;
			return false;
		}

		if (!source.ConsumeWhitespace(ref i))
		{
			parameters = null;
			function = null;
			return false;
		}

		// consume (
		if (source[i] != '(')
		{
			parameters = null;
			function = null;
			return false;
		}

		i++;

		// parse list of parameters - all value expressions
		parameters = new List<ValueExpressionNode>();
		var done = false;

		while (i < source.Length && !done)
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				parameters = null;
				function = null;
				return false;
			}

			if (!ValueExpressionParser.TryParse(source, ref i, out var parameter, options)) break;

			parameters.Add(parameter);

			if (!source.ConsumeWhitespace(ref i))
			{
				parameters = null;
				function = null;
				return false;
			}

			switch (source[i])
			{
				case ')':
					done = true;
					i++;
					break;
				case ',':
					i++;
					break;
				default:
					parameters = null;
					function = null;
					return false;
			}
		}

		if (!FunctionRepository.TryGet(name, out function))
		{
			parameters = null;
			function = null;
			return false;
		}

		var parameterTypes = parameters.Select(x => x.GetParameterType()).ToList();
		if (options.StrictTypeChecking)
		{
			if (!function.ParameterSets.Any(x => x.SequenceEqual(parameterTypes, EqualOrUnspecifiedEqualityComparer.Instance)))
			{
				parameters = null;
				function = null;
				return false;
			}
		}
		else if (function.ParameterSets.All(x => x.Count() != parameterTypes.Count))
		{
			parameters = null;
			function = null;
			return false;
		}

		//if (function.MinArgumentCount > parameters.Count ||
		//    parameters.Count > function.MaxArgumentCount)
		//{
		//	parameters = null;
		//	function = null;
		//	return false;
		//}

		index = i;
		return true;
	}

	private static ParameterType GetParameterType(this ValueExpressionNode valueNode)
	{
		if (valueNode is not LiteralExpressionNode literal) return ParameterType.Unspecified;

		return GetParameterType(literal.Value);
	}

	private static ParameterType GetParameterType(this JsonNode? node)
	{
		if (node is null) return ParameterType.Null;
		if (node is JsonArray) return ParameterType.Array;
		if (node is JsonObject) return ParameterType.Object;
		if (node is JsonValue value)
		{
			var obj = value.GetValue<object>();
			if (obj is JsonNull) return ParameterType.Null;
			if (obj is JsonElement element) return GetParameterType(element);
			var objType = obj.GetType();
			if (objType.IsNumber()) return ParameterType.Number;
			if (obj is string) return ParameterType.String;
			if (obj is bool) return ParameterType.Boolean;
		}

		throw new ArgumentOutOfRangeException(nameof(node));
	}

	private static ParameterType GetParameterType(JsonElement element) =>
		element.ValueKind switch
		{
			JsonValueKind.Object => ParameterType.Object,
			JsonValueKind.Array => ParameterType.Array,
			JsonValueKind.String => ParameterType.String,
			JsonValueKind.Number => ParameterType.Number,
			JsonValueKind.True => ParameterType.Boolean,
			JsonValueKind.False => ParameterType.Boolean,
			JsonValueKind.Null => ParameterType.Null,
			_ => throw new ArgumentOutOfRangeException(nameof(element.ValueKind), element.ValueKind, null)
		};

}