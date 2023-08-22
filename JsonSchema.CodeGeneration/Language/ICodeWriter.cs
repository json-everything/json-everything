using System.Text;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration.Language;

/// <summary>
/// Converts the type model into code text.
/// </summary>
public interface ICodeWriter
{
	/// <summary>
	/// Converts the type model into code text.
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