using System.Text;

namespace Json.Path;

public interface IHaveShorthand
{
	string ToShorthandString();
	void AppendShorthandString(StringBuilder builder);
}