using System.Linq;
using System.Text;

namespace Json.Path;

public class PathSegment
{
	public ISelector[] Selectors { get; set; }
	public bool IsShorthand { get; set; }

	public override string ToString()
	{
		return IsShorthand
			? $"{((IHaveShorthand)Selectors[0]).ToShorthandString()}"
			: $"[{string.Join(',', Selectors.Select(x => x.ToString()))}]";
	}

	public void BuildString(StringBuilder builder)
	{
		if (IsShorthand)
		{
			builder.Append(((IHaveShorthand)Selectors[0]).ToShorthandString());
			return;
		}

		builder.Append('[');

		Selectors[0].BuildString(builder);

		for (int i = 1; i < Selectors.Length; i++)
		{
			builder.Append(',');
			Selectors[i].BuildString(builder);
		}

		builder.Append(']');
	}
}