using System.Text.RegularExpressions;
using JsonEverythingNet.Shared;
using Markdig;
using Markdig.SyntaxHighlighting;

namespace JsonEverythingNet.Services
{
	public static class AnchorRegistry
	{
		private static readonly Dictionary<string, string> _registry = new();
		private static readonly Dictionary<string, string> _firstLinks = new();

		public static async Task RegisterDocs(HttpClient client)
		{
			await RegisterAnchors(client, "json-more");

			await RegisterAnchors(client, "playground/patch");
			await RegisterAnchors(client, "json-patch");

			await RegisterAnchors(client, "playground/path");
			await RegisterAnchors(client, "json-path");

			await RegisterAnchors(client, "playground/pointer");
			await RegisterAnchors(client, "json-pointer");

			await RegisterAnchors(client, "playground/logic");
			await RegisterAnchors(client, "json-logic");

			await RegisterAnchors(client, "playground/schema");
			await RegisterAnchors(client, "schema-basics");
			await RegisterAnchors(client, "schema-datagen");
			await RegisterAnchors(client, "schema-generation");
			await RegisterAnchors(client, "schema-vocabs");
			await RegisterAnchors(client, "vocabs-data");
			await RegisterAnchors(client, "vocabs-unique-keys");
		}

		private static async Task RegisterAnchors(HttpClient client, string page)
		{
			var markdown = await client.GetStringAsync($"/md/{page}.md");
			var pipeline = new MarkdownPipelineBuilder()
				.UseAdvancedExtensions()
				.UseSyntaxHighlighting()
				.Build();
			var html = Markdown.ToHtml(markdown, pipeline);

			var matches = Docs.HeaderPattern.Matches(html);
			var first = true;

			foreach (Match match in matches)
			{
				var href = match.Groups[2].Value;

				_registry[href] = page;
				if (!first) continue;

				_firstLinks[page] = href;
				first = false;
			}
		}

		public static string? GetPageForFragment(string fragment)
		{
			_registry.TryGetValue(fragment, out var ownerDoc);

			return ownerDoc;
		}

		public static string GetFirstFragment(string page)
		{
			_firstLinks.TryGetValue(page, out var link);

			return link!;
		}

		public static string GetHrefFromText(string text)
		{
			return text.Replace(" ", "-").ToLowerInvariant();
		}
	}
}