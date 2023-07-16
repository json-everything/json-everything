using System.Reflection;
using System.Text.RegularExpressions;
using ApiDocsGenerator.MarkdownGen;
using ApiDocsGenerator.MarkdownGen.MarkdownWriters;

namespace ApiDocsGenerator;

public static class ApiDocGenerationService
{
	public static async Task<IEnumerable<(string path, string doc)>> GenerateForAssemblyContaining(Type type, string index)
	{
		var assembly = type.Assembly;
		var allTypes = assembly.ExportedTypes.OrderBy(x => x.Name);

		var docs = await Task.WhenAll(allTypes.Select(async (x, i) =>
		(
			path: x.Name,
			doc: await Task.Run(() => GenerateForType(x, $"{index}.{i:d3}"))
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
title: __title
bookmark: {asmName}
permalink: /api/{asmName}/:title/
folder: true
order: ""{index}""
version: ""{GetVersion(type)}""
---
";
		var close = @$"---
title: __close
permalink: /api/{asmName}/:title/
close: true
order: ""{index}.99""
---
";

		return (title, close);
	}

	private static string GetVersion(Type type)
	{
		var attribute = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		var version = Regex.Match(attribute!.InformationalVersion, @"\d+\.\d+\.\d+(-[^+]+)?").Value;
		return version;
	}
}
