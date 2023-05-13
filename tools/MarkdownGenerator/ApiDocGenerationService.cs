using ApiDocsGenerator.MarkdownGen;
using ApiDocsGenerator.MarkdownGen.MarkdownWriters;

namespace ApiDocsGenerator;

public static class ApiDocGenerationService
{
	public static async Task<IEnumerable<(string path, string doc)>> GenerateForAssemblyContaining(Type type, string index)
	{
		var assembly = type.Assembly;
		var allTypes = assembly.ExportedTypes;

		var docs = await Task.WhenAll(allTypes.Select(async (x, i) =>
		(
			path: x.Name,
			doc: await GenerateForType(x, $"{index}.{i:d2}")
		)));

		return docs;
	}

	private static async Task<string> GenerateForType(Type type, string index)
	{
		var writer = new GithubMarkdownWriter();
		await DocumentationGenerator.GenerateMarkdown(type, writer, index);
		return writer.FullText;
	}

	public static (string title, string close) GenerateFolderMarkersForNamespace(Type type, string index)
	{
		var asmName = type.Assembly.GetName().Name;

		var title = @$"---
bookmark: {asmName}
permalink: /api/{asmName}/:title/
folder: true
order: ""{index}""
---
";
		var close = @$"---
permalink: /api/JsonSchema.Net/:title/
close: true
order: ""{index}.99""
---
";

		return (title, close);
	}
}
