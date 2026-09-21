using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yaml2JsonNode;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Provides registration for OpenAPI description generation.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
	/// <summary>
	/// Builds an OpenAPI description of the application's API surface and publishes it.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configure">
	/// An optional delegate that edits the description before it is published.  Use this to
	/// supply anything the analyzer cannot infer — contact details, servers, security
	/// schemes, descriptions.
	/// </param>
	/// <param name="configureOptions">An optional delegate that configures publication.</param>
	/// <returns>The same <see cref="IServiceCollection"/>, so calls can be chained.</returns>
	/// <remarks>
	/// The description is assembled from the fragments emitted into each assembly that
	/// references this package, so controllers declared in a class library are included
	/// without that library knowing about the host.
	/// </remarks>
	[RequiresUnreferencedCode("Reflects over loaded assemblies to collect generated OpenAPI fragments.")]
	public static IServiceCollection AddOpenApi(
		this IServiceCollection services,
		Action<OpenApiDocument>? configure = null,
		Action<OpenApiOptions>? configureOptions = null)
	{
		var options = new OpenApiOptions();
		configureOptions?.Invoke(options);

		services.TryAddSingleton(options);

		services.TryAddSingleton(_ =>
		{
			var fragments = OpenApiDocumentBuilder.CollectFragments();
			var document = OpenApiDocumentBuilder.Build(fragments, options);

			configure?.Invoke(document);

			return document;
		});

		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<IStartupFilter, OpenApiStartupFilter>());

		return services;
	}
}

/// <summary>
/// Publishes the OpenAPI description once the application has been built.
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
			var document = builder.ApplicationServices.GetRequiredService<OpenApiDocument>();

			WriteFiles(document);

			if (_options.Publish)
				builder.Use(async (context, proceed) => await Serve(context, proceed, document));

			next(builder);
		};

	private void WriteFiles(OpenApiDocument document)
	{
		foreach (var path in _options.OutputPaths)
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(path));
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllText(path, Render(document, IsYamlPath(path)));
		}
	}

	private async System.Threading.Tasks.Task Serve(HttpContext context, Func<System.Threading.Tasks.Task> proceed, OpenApiDocument document)
	{
		var path = context.Request.Path.Value ?? string.Empty;

		if (!TryMatchRoute(path, out var yaml))
		{
			await proceed();
			return;
		}

		// With no extension, the caller's `Accept` header decides.
		yaml ??= PrefersYaml(context.Request.Headers.Accept.ToString());

		context.Response.ContentType = yaml.Value
			? "application/yaml; charset=utf-8"
			: "application/json; charset=utf-8";

		await context.Response.WriteAsync(Render(document, yaml.Value));
	}

	private bool TryMatchRoute(string path, out bool? yaml)
	{
		yaml = null;

		if (path.Equals(_options.Route, StringComparison.OrdinalIgnoreCase)) return true;

		if (!path.StartsWith(_options.Route, StringComparison.OrdinalIgnoreCase)) return false;

		var extension = path.Substring(_options.Route.Length);

		switch (extension.ToLowerInvariant())
		{
			case ".json":
				yaml = false;
				return true;
			case ".yaml":
			case ".yml":
				yaml = true;
				return true;
			default:
				return false;
		}
	}

	private static bool PrefersYaml(string accept) =>
		accept.Contains("yaml", StringComparison.OrdinalIgnoreCase);

	private static bool IsYamlPath(string path)
	{
		var extension = Path.GetExtension(path).ToLowerInvariant();

		return extension is ".yaml" or ".yml";
	}

	private static string Render(OpenApiDocument document, bool yaml)
	{
		var options = new JsonSerializerOptions { WriteIndented = true };

		return yaml
			? YamlSerializer.Serialize(document, options)
			: JsonSerializer.Serialize(document, options);
	}
}
