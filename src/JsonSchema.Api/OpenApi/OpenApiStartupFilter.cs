using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Yaml2JsonNode;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Publishes the description, and the reference page, once the application has been built.
/// </summary>
internal class OpenApiStartupFilter : IStartupFilter
{
	private readonly IReadOnlyList<OpenApiOptions> _descriptions;

	public OpenApiStartupFilter(IEnumerable<OpenApiOptions> descriptions)
	{
		_descriptions = [.. descriptions];
	}

	public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
		builder =>
		{
			// One page covers every published description, so it is rendered once from the
			// description that declares it rather than per description.
			var host = _descriptions.FirstOrDefault(x => x.InteractivePath is not null);

			var page = host is null
				? null
				: OpenApiPageRenderer.Render(host, [.. _descriptions.Where(x => x.DocumentPath is not null)]);

			foreach (var options in _descriptions)
			{
				WriteFiles(options);

				if (options.DocumentPath is null) continue;

				// Captured per description, so each registration serves its own.
				var current = options;
				builder.Use((HttpContext context, Func<Task> proceed) => Serve(current, context, proceed));
			}

			if (host is not null)
				builder.Use((HttpContext context, Func<Task> proceed) => ServePage(host, context, proceed, page!));

			next(builder);
		};

	private static void WriteFiles(OpenApiOptions options)
	{
		if (options.FileOutputPath is null) return;

		var directory = Path.GetDirectoryName(Path.GetFullPath(options.FileOutputPath));
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);

		if (options.DocumentFormats.HasFlag(OpenApiFormats.Json))
			File.WriteAllText(options.FileOutputPath + ".json", Render(options, false));

		if (options.DocumentFormats.HasFlag(OpenApiFormats.Yaml))
			File.WriteAllText(options.FileOutputPath + ".yaml", Render(options, true));
	}

	private static async Task ServePage(OpenApiOptions options, HttpContext context, Func<Task> proceed, string page)
	{
		if (!HttpMethods.IsGet(context.Request.Method) ||
			!string.Equals(context.Request.Path.Value, options.InteractivePath, StringComparison.OrdinalIgnoreCase))
		{
			await proceed();
			return;
		}

		context.Response.ContentType = "text/html; charset=utf-8";

		await context.Response.WriteAsync(page);
	}

	private static async Task Serve(OpenApiOptions options, HttpContext context, Func<Task> proceed)
	{
		if (!HttpMethods.IsGet(context.Request.Method))
		{
			await proceed();
			return;
		}

		var path = context.Request.Path.Value ?? string.Empty;

		if (!TryMatchDocument(options, path, out var yaml))
		{
			await proceed();
			return;
		}

		context.Response.ContentType = yaml
			? "application/yaml; charset=utf-8"
			: "application/json; charset=utf-8";

		await context.Response.WriteAsync(Render(options, yaml));
	}

	private static bool TryMatchDocument(OpenApiOptions options, string path, out bool yaml)
	{
		yaml = false;

		if (options.DocumentPath is null) return false;
		if (!path.StartsWith(options.DocumentPath, StringComparison.OrdinalIgnoreCase)) return false;

		var extension = path[options.DocumentPath.Length..].ToLowerInvariant();

		if (extension == ".json")
			return options.DocumentFormats.HasFlag(OpenApiFormats.Json);

		if (IsYaml(extension))
		{
			yaml = true;
			return options.DocumentFormats.HasFlag(OpenApiFormats.Yaml);
		}

		return false;
	}

	private static bool IsYaml(string extension) => extension is ".yaml" or ".yml";

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "The description is serialized through a serializer context; the YAML writer is handed an already-materialized node.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "The description is serialized through a serializer context; the YAML writer is handed an already-materialized node.")]
	private static string Render(OpenApiOptions options, bool yaml)
	{
		var json = JsonSerializer.Serialize(options.Document, _indented.OpenApiDocument);
		if (!yaml) return json;

		var node = JsonNode.Parse(json)!.ToYamlNode();
		return YamlSerializer.Serialize(node, null);
	}

	private static readonly ApiSerializerContext _indented =
		new(new JsonSerializerOptions { WriteIndented = true });
}