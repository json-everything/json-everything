using System.Text;

namespace Json.Path.Expressions;

internal abstract class ExpressionNode
{
	public abstract void BuildString(StringBuilder builder);
}