using System.Collections.Generic;

namespace Json.JsonE.Operators;

internal static class CommonErrors
{
	public static string UndefinedProperties(string op, IEnumerable<string> extraProperties) => 
		$"{op} has undefined properties: {string.Join(", ", extraProperties)}";

	public static string IncorrectValueType(string op, string expectedType) =>
		$"{op} value must evaluate to an {expectedType}";
}