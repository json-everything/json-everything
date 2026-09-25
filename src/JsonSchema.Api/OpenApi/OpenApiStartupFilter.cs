using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Publishes the description, and the reference page, once the application has been built.
/// </summary>
internal class OpenApiStartupFilter : IStartupFilter
{
	private readonly OpenApiOptions _options;

	public OpenApiStartupFilter(OpenApiOptions options)
	{
		_options = options;
	}

	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
		builder =>
		{
			WriteFiles();

			var page = _options.InteractivePath is null
				? null
				: OpenApiPageRenderer.Render(_options);

			if (_options.DocumentPath is not null || page is not null)
				builder.Use((context, proceed) => Serve(context, proceed, page));

			next(builder);
		};

	private void WriteFiles()
	{
		if (_options.FileOutputPath is null) return;

		var directory = Path.GetDirectoryName(Path.GetFullPath(_options.FileOutputPath));
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);

		if (_options.DocumentFormats.HasFlag(OpenApiFormats.Json))
			File.WriteAllText(_options.FileOutputPath + ".json", Render(false));

		if (_options.DocumentFormats.HasFlag(OpenApiFormats.Yaml))
			File.WriteAllText(_options.FileOutputPath + ".yaml", Render(true));
	}

	private async Task Serve(HttpContext context, Func<Task> proceed, string? page)
	{
		if (!HttpMethods.IsGet(context.Request.Method))
		{
			await proceed();
			return;
		}

		var path = context.Request.Path.Value ?? string.Empty;

		if (page is not null &&
		    string.Equals(path, _options.InteractivePath, StringComparison.OrdinalIgnoreCase))
		{
			context.Response.ContentType = "text/html; charset=utf-8";
			await context.Response.WriteAsync(page);
			return;
		}

		if (!TryMatchDocument(path, out var yaml))
		{
			await proceed();
			return;
		}

		context.Response.ContentType = yaml
			? "application/yaml; charset=utf-8"
			: "application/json; charset=utf-8";

		await context.Response.WriteAsync(Render(yaml));
	}

	private bool TryMatchDocument(string path, out bool yaml)
	{
		yaml = false;

		if (_options.DocumentPath is null) return false;
		if (!path.StartsWith(_options.DocumentPath, StringComparison.OrdinalIgnoreCase)) return false;

		var extension = path.Substring(_options.DocumentPath.Length).ToLowerInvariant();

		if (extension == ".json")
			return _options.DocumentFormats.HasFlag(OpenApiFormats.Json);

		if (IsYaml(extension))
		{
			yaml = true;
			return _options.DocumentFormats.HasFlag(OpenApiFormats.Yaml);
		}

		return false;
	}

	private static bool IsYaml(string extension) => extension is ".yaml" or ".yml";

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "The description is serialized through a serializer context; the YAML writer is handed an already-materialized node.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "The description is serialized through a serializer context; the YAML writer is handed an already-materialized node.")]
	private string Render(bool yaml)
	{
		var json = JsonSerializer.Serialize(_options.Document, _indented.OpenApiDocument);
		if (!yaml) return json;

		var node = JsonNode.Parse(json)!.ToYamlNode();
		return YamlSerializer.Serialize(node, configure: null);
	}

	private static readonly ApiSerializerContext _indented =
		new(new JsonSerializerOptions { WriteIndented = true });
}