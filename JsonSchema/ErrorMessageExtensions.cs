using System.Linq;

namespace Json.Schema;

public static class ErrorMessageExtensions
{
	public static string ReplaceTokens(this string message, params (string token, object value)[] parameters)
	{
		return parameters.Aggregate(message, (current, parameter) => current.Replace($"[[{parameter.token}]]", parameter.value.ToString()));
	}
}