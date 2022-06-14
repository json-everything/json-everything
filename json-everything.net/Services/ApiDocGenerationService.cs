using JsonEverythingNet.Services.MarkdownGen;
using JsonEverythingNet.Services.MarkdownGen.MarkdownWriters;

namespace JsonEverythingNet.Services;

public class ApiDocGenerationService
{
	public string GenerateForType(Type type)
	{
		var writer = new GithubMarkdownWriter();
		DocumentationGenerator.GenerateMarkdown(type, writer);
		return writer.FullText;
	}

}