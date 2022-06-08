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
			await Task.WhenAll(
				RegisterAnchors(client, "json-more"),

				RegisterAnchors(client, "playground/patch"),
				RegisterAnchors(client, "json-patch"),

				RegisterAnchors(client, "playground/path"),
				RegisterAnchors(client, "json-path"),

				RegisterAnchors(client, "playground/pointer"),
				RegisterAnchors(client, "json-pointer"),

				RegisterAnchors(client, "playground/logic"),
				RegisterAnchors(client, "json-logic"),

				RegisterAnchors(client, "playground/schema"),
				RegisterAnchors(client, "schema-basics"),
				RegisterAnchors(client, "schema-datagen"),
				RegisterAnchors(client, "schema-generation"),
				RegisterAnchors(client, "schema-vocabs"),
				RegisterAnchors(client, "vocabs-data"),
				RegisterAnchors(client, "vocabs-unique-keys")
			);
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