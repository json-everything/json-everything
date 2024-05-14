using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal class JsonExpression
{
	public ExpressionNode Expression { get; }

	private JsonExpression(ExpressionNode expression)
	{
		Expression = expression;
	}

	public static JsonExpression Create(ExpressionNode expression)
	{
		return new JsonExpression(expression);
	}

	public static implicit operator JsonValue(JsonExpression expression)
	{
		return JsonValue.Create(expression, JsonESerializerContext.Default.JsonExpression)!;
	}
}