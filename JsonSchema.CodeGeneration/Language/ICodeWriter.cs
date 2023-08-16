using System.Text;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration.Language;

public interface ICodeWriter
{
	void Write(StringBuilder builder, TypeModel model);
}

public static class CodeWriters
{
	public static readonly ICodeWriter CSharp = new CSharpCodeWriter();
}