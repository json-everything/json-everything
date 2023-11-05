using System.Text;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal abstract class ExpressionNode
{
	public abstract JsonNode? Evaluate(EvaluationContext context);
	public abstract void BuildString(StringBuilder builder);


}