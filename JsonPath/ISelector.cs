using System.Text;

namespace Json.Path;

public interface ISelector
{
	void BuildString(StringBuilder builder);
}