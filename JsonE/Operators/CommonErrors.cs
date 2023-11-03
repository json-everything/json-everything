using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.JsonE.Operators;

internal static class CommonErrors
{
	public static string UndefinedProperties(string op, IEnumerable<string> extraProperties) => 
		$"{op} has undefined properties: {string.Join(" ", extraProperties.OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase))}";

	public static string IncorrectValueType(string op, string expectedType) =>
		$"{op} value must evaluate to {expectedType}";

	public static string EndOfInput(int i) =>
		$"Unexpected end of input at index {i}";
}