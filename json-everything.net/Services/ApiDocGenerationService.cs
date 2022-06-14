using System.Xml;
using JsonEverythingNet.Services.MarkdownGen;
using JsonEverythingNet.Services.MarkdownGen.MarkdownWriters;

namespace JsonEverythingNet.Services;

public class ApiDocGenerationService
{
	private readonly HttpClient _client;

	public ApiDocGenerationService(HttpClient client)
	{
		_client = client;
	}

	public async Task<string> GenerateForType(Type type)
	{
		var writer = new GithubMarkdownWriter();
		await DocumentationGenerator.GenerateMarkdown(type, writer, _client);
		return writer.FullText;
	}

}