using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;
using Json.Schema.Api;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Provides registration for OpenAPI description generation.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
	/// <summary>
	/// Describes the application's API surface in OpenAPI, and publishes the description.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configure">
	/// An optional delegate that configures the description and how it is published.
	/// </param>
	/// <returns>The same <see cref="IServiceCollection"/>, so calls can be chained.</returns>
	/// <remarks>
	/// The description is assembled from the fragments each assembly registers as it loads,
	/// so controllers declared in a class library are included without that library knowing
	/// about the host.
	/// </remarks>
	/// <example>
	/// <code>
	/// builder.Services.AddOpenApi(c =>
	/// {
	///     c.Document.Info.Description = "The pet store API.";
	///     c.DocumentPath = "/openapi";
	///     c.InteractivePath = "/openapi/reference";
	/// });
	/// </code>
	/// </example>
	public static IServiceCollection AddOpenApi(
		this IServiceCollection services,
		Action<OpenApiOptions>? configure = null)
	{
		var document = OpenApiDocumentBuilder.Build(OpenApiDocumentBuilder.CollectFragments());
		var options = new OpenApiOptions(document);

		configure?.Invoke(options);
		options.Validate();

		services.TryAddSingleton(options);
		services.TryAddSingleton(options.Document);

		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<IStartupFilter, OpenApiStartupFilter>());

		return services;
	}
}

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
		foreach (var path in _options.OutputPaths)
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(path));
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllText(path, Render(IsYaml(Path.GetExtension(path))));
		}
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

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
		Justification = "The description is serialized through a serializer context; the YAML writer is handed an already-materialized node.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
		Justification = "The description is serialized through a serializer context; the YAML writer is handed an already-materialized node.")]
	private string Render(bool yaml)
	{
		var json = JsonSerializer.Serialize(_options.Document, _indented.OpenApiDocument);

		if (!yaml) return json;

		// Converting the node and writing that avoids the generic serializer, which is not
		// trim-safe; the JSON above already went through the serializer context.
		YamlNode node = JsonNode.Parse(json)!.ToYamlNode();

		// Naming the second parameter selects the `YamlNode` overload; the generic one
		// takes `JsonSerializerOptions` there, so the call would otherwise be ambiguous.
		return YamlSerializer.Serialize(node, configure: null);
	}

	private static readonly ApiSerializerContext _indented =
		new(new JsonSerializerOptions { WriteIndented = true });
}
