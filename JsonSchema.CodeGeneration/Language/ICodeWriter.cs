using System.Text;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration.Language;

/// <summary>
/// Converts the type model into code text.
/// </summary>
public interface ICodeWriter
{
	/// <summary>
	/// Transforms a name as it appears in a JSON string into a language-appropriate type or member name.
	/// </summary>
	/// <param name="original">The JSON string.</param>
	/// <returns>The transformed name, or null if the string is unsupported.</returns>
	string? TransformName(string? original);

	/// <summary>
	/// Converts a single type model into code text.
	/// </summary>
	/// <param name="builder">A string builder.</param>
	/// <param name="model">A type model.</param>
	void Write(StringBuilder builder, TypeModel model);

}

/// <summary>
/// Provides predefined code writers.
/// </summary>
public static class CodeWriters
{
	/// <summary>
	/// A code writer to generate C# text.
	/// </summary>
	public static readonly ICodeWriter CSharp = new CSharpCodeWriter();
}