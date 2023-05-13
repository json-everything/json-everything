using ApiDocsGenerator.MarkdownGen;
using ApiDocsGenerator.MarkdownGen.MarkdownWriters;

namespace ApiDocsGenerator;

public static class ApiDocGenerationService
{
	public static async Task<IEnumerable<(string[] path, string doc)>> GenerateForAssemblyContaining(Type type)
	{
		var assembly = type.Assembly;
		var allTypes = assembly.ExportedTypes;

		var docs = await Task.WhenAll(allTypes.Select(async x =>
		(
			path: x.FullName!.Replace(type.Namespace!, string.Empty).Split('.'),
			doc: await GenerateForType(x)
		)));

		return docs;
	}

	private static async Task<string> GenerateForType(Type type)
	{
		var writer = new GithubMarkdownWriter();
		await DocumentationGenerator.GenerateMarkdown(type, writer);
		return writer.FullText;
	}

}
