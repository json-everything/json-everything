using System.Text;

namespace Json.Path;

/// <summary>
/// Implemented by selectors which have a shorthand syntax.
/// </summary>
public interface IHaveShorthand
{
	/// <summary>
	/// Gets the shorthand syntax string.
	/// </summary>
	/// <returns>The shorthand syntax string.</returns>
	string ToShorthandString();
	/// <summary>
	/// Appends the shorthand syntax string to a string builder.
	/// </summary>
	void AppendShorthandString(StringBuilder builder);
}