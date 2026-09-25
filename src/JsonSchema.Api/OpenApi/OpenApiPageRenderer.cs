using System;
using System.IO;
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
	/// <param name="options">The options.</param>
	/// <returns>The page markup.</returns>
	public static string Render(OpenApiOptions options)
	{
		var config = JsonSerializer.Serialize(
			new PageConfig { DocumentUrl = options.DocumentPath + ".json" },
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
	/// Gets or sets the URL the page fetches the description from.
	/// </summary>
	public string DocumentUrl { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PageConfig))]
internal partial class PageConfigContext : JsonSerializerContext;
