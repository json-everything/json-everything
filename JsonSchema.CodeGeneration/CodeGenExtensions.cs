using System.Text;
using Json.Schema.CodeGeneration.Language;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration;

/// <summary>
/// Generates code from a <see cref="JsonSchema"/>.
/// </summary>
public static class CodeGenExtensions
{
	/// <summary>
	/// Generates code from a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="schema">The JSON Schema object.</param>
	/// <param name="codeWriter">The writer for the output language.</param>
	/// <param name="options">Evaluation options.</param>
	/// <returns></returns>
	public static string GenerateCode(this JsonSchema schema, ICodeWriter codeWriter, EvaluationOptions? options = null)
	{
		options = EvaluationOptions.From(options ?? EvaluationOptions.Default);
		options.SchemaRegistry.Register(schema);

		var generationCache = new GenerationCache();
		var model = schema.GenerateCodeModel(options, generationCache);

		generationCache.FillPlaceholders();

		var sb = new StringBuilder();

		codeWriter.Write(sb, model);

		return sb.ToString();
	}
}
