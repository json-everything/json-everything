using System.Text;

namespace Json.Path;

internal interface IHaveShorthand
{
	string ToShorthandString();
	void AppendShorthandString(StringBuilder builder);
}