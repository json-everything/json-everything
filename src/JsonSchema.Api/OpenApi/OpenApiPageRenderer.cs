using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Assembles the documentation page from its embedded templates.
/// </summary>
internal static class OpenApiPageRenderer
{
	private const string _prefix = "Json.Schema.Api.OpenApi.Page.";

	private static readonly Lazy<string> _template = new(() => ReadResource("page.html"));
	private static readonly Lazy<string> _styles = new(() => ReadResource("console.css"));
	private static readonly Lazy<string> _script = new(() => ReadResource("console.js"));

	/// <summary>
	/// Renders the page.
	/// </summary>
	/// <param name="options">The options carrying the page's own configuration.</param>
	/// <param name="descriptions">
	/// Every published description, in the order they appear in the page's selector.
	/// </param>
	/// <returns>The page markup.</returns>
	public static string Render(OpenApiOptions options, IReadOnlyList<OpenApiOptions> descriptions)
	{
		var config = JsonSerializer.Serialize(
			new PageConfig
			{
				Documents =
				[
					// Labeled by title, which is what a reader recognizes; the name is an
					// internal key and may not be set on the default description at all.
					.. descriptions.Select(x => new PageDocument
					{
						Name = string.IsNullOrWhiteSpace(x.Document.Info.Title)
							? x.Name ?? "API"
							: x.Document.Info.Title,
						Url = x.DocumentPath + ".json"
					})
				]
			},
			PageConfigContext.Default.PageConfig);

		var head = string.IsNullOrEmpty(options.StylesheetUrl)
			? string.Empty
			: $"\t<link rel=\"stylesheet\" href=\"{HtmlEncoder.Default.Encode(options.StylesheetUrl!)}\">";

		return _template.Value
			.Replace("{{title}}", HtmlEncoder.Default.Encode(options.Document.Info.Title))
			.Replace("{{styles}}", _styles.Value)
			.Replace("{{head}}", head)
			.Replace("{{config}}", config)
			.Replace("{{script}}", _script.Value);
	}

	private static string ReadResource(string name)
	{
		var assembly = typeof(OpenApiPageRenderer).GetTypeInfo().Assembly;

		using var stream = assembly.GetManifestResourceStream(_prefix + name)
			?? throw new InvalidOperationException($"The `{name}` page template is missing from this assembly.");

		using var reader = new StreamReader(stream);

		return reader.ReadToEnd();
	}
}

/// <summary>
/// The configuration the page reads at startup.
/// </summary>
internal class PageConfig
{
	/// <summary>
	/// Gets or sets the descriptions the page can show.  The first is shown on load, and the
	/// selector is hidden when there is only one.
	/// </summary>
	public IReadOnlyList<PageDocument> Documents { get; set; } = [];
}

/// <summary>
/// One entry in the page's description selector.
/// </summary>
internal class PageDocument
{
	/// <summary>
	/// Gets or sets the label shown in the selector.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the URL the description is fetched from.
	/// </summary>
	public string Url { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PageConfig))]
internal partial class PageConfigContext : JsonSerializerContext;
