using System.Text;
using Json.Schema.CodeGeneration.Language;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration;

public static class CodeGenExtensions
{
	public static string GenerateCode(this JsonSchema schema, ICodeWriter codeGenerator)
	{
		var model = schema.GenerateCodeModel();

		var sb = new StringBuilder();

		codeGenerator.Write(sb, model);

		return sb.ToString();
	}
}
