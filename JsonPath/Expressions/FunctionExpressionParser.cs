using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

		public bool Equals(ParameterType parameterType, ParameterType argumentType)
		{
			if (parameterType == ParameterType.Unspecified || argumentType == ParameterType.Unspecified) return true;
			if (parameterType == argumentType) return true;

			if (parameterType == ParameterType.Value && (argumentType ^ ParameterType.Value) != 0) return true;
			if (argumentType == ParameterType.Value && (parameterType ^ ParameterType.Value) != 0) return true;

			return false;
		}

		public int GetHashCode(ParameterType obj)
		{
			return 0;
		}
	}

	public static bool TryParseFunction(ReadOnlySpan<char> source, ref int index, [NotNullWhen(true)] out List<ValueExpressionNode>? arguments, [NotNullWhen(true)] out IPathFunctionDefinition? function, PathParsingOptions options)
	{
		int i = index;

		if (!source.ConsumeWhitespace(ref i))
		{
			arguments = null;
			function = null;
			return false;
		}

		// parse function name
		if (!source.TryParseName(ref i, out var name))
		{
			arguments = null;
			function = null;
			return false;
		}

		if (!source.ConsumeWhitespace(ref i))
		{
			arguments = null;
			function = null;
			return false;
		}

		// consume (
		if (source[i] != '(')
		{
			arguments = null;
			function = null;
			return false;
		}

		i++;

		// parse list of arguments - all value expressions
		// TODO: This won't work; args can be logical or nodelist functions
		arguments = new List<ValueExpressionNode>();
		var done = false;

		while (i < source.Length && !done)
		{
			if (!source.ConsumeWhitespace(ref i))
			{
				arguments = null;
				function = null;
				return false;
			}

			if (!ValueExpressionParser.TryParse(source, ref i, out var parameter, options)) break;

			arguments.Add(parameter);

			if (!source.ConsumeWhitespace(ref i))
			{
				arguments = null;
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
					arguments = null;
					function = null;
					return false;
			}
		}

		if (!FunctionRepository.TryGet(name, out function))
		{
			arguments = null;
			function = null;
			return false;
		}

		var argumentTypes = arguments.Select(x => x.GetParameterType()).ToList();
		if (!function.ParameterSets.Any(x => x.SequenceEqual(argumentTypes, EqualOrUnspecifiedEqualityComparer.Instance)))
		{
			arguments = null;
			function = null;
			return false;
		}

		index = i;
		return true;
	}

	private static ParameterType GetParameterType(this ValueExpressionNode valueNode)
	{
		if (valueNode is ValueFunctionExpressionNode function)
			return function.Function.ReturnType switch
			{
				FunctionType.Unspecified => ParameterType.Unspecified,
				FunctionType.Value => ParameterType.Value,
				FunctionType.Logical => ParameterType.Logical,
				FunctionType.Nodelist => ParameterType.Nodelist,
				_ => throw new ArgumentOutOfRangeException()
			};

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