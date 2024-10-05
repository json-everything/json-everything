using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.JsonE.Operators;

internal static class CommonErrors
{
	public static string UndefinedProperties(string op, IEnumerable<string> extraProperties) => 
		$"{op} has undefined properties: {string.Join(" ", extraProperties.OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase))}";

	public static string IncorrectOperatorArgCount(string op, int expectedCount) => 
		$"{op} must have exactly {expectedCount.ToWords()} properties";

	public static string IncorrectValueType(string op, string expectedType) =>
		$"{op} value must evaluate to {expectedType}";

	public static string IncorrectArgType(string op) => $"invalid arguments to builtin: {op}";

	public static string IncorrectArgType(string op, string explanation) => $"invalid arguments to builtin: {op}: {explanation}";

	public static string EndOfInput(int i) => $"Unexpected end of input at index {i}";

	public static string SortSameType() => "$sorted values to be sorted must have the same type";

	public static string WrongToken(string token) => $"Found: {token} token, expected one of: !, (, +, -, [, false, identifier, null, number, string, true, {{";

	public static string WrongToken(char token) => $"Found: {token} token, expected one of: !, (, +, -, [, false, identifier, null, number, string, true, {{";

	public static string WrongToken(string token, params string[] expected) => $"Found: {token} token, expected one of: {string.Join(",", expected)}";

	public static string WrongToken(char token, params string[] expected) => $"Found: {token} token, expected one of: {string.Join(",", expected)}";

	public static string EndOfInput() => "Unexpected end of input";

	private static readonly List<string> _words = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"]; // probably shouldn't need more than this.
	private static string ToWords(this int count) => _words[count];
}