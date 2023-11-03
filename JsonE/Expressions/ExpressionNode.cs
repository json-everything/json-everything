using System.Text;

namespace Json.JsonE.Expressions;

internal abstract class ExpressionNode
{
	public abstract void BuildString(StringBuilder builder);
}