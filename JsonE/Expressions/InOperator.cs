using System;
using System.Linq;
using System.Text.Json.Nodes;
using Json.JsonE.Operators;
using Json.More;

namespace Json.JsonE.Expressions;

internal class InOperator : IBinaryOperator
{
	public int Precedence => 4;

	public JsonNode? Evaluate(JsonNode? left, JsonNode? right)
	{
		return right switch
		{
			JsonArray arr => arr.Contains(left, JsonNodeEqualityComparer.Instance),
			JsonObject obj => left is JsonValue val && val.TryGetValue(out string? key)
				? obj.Any(x => string.Equals(key, x.Key, StringComparison.Ordinal))
				: throw new InterpreterException("only strings can be found in objects"),
			JsonValue vRight when vRight.TryGetValue(out string? sRight) &&
			                      left is JsonValue vLeft &&
			                      vLeft.TryGetValue(out string? sLeft) => sRight.Contains(sLeft),
			_ => throw new BuiltInException(CommonErrors.IncorrectArgType("in"))
		};
	}

	public override string ToString()
	{
		return " in ";
	}
}