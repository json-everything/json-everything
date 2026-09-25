using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

		if (options.AddValidation)
			AddValidation(services);

		services.AddSingleton(options);
		services.AddSingleton(options.Document);

		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<IStartupFilter, OpenApiStartupFilter>());

		return services;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Reached only when the consumer leaves `AddValidation` on, which opts into runtime schema generation.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Reached only when the consumer leaves `AddValidation` on, which opts into runtime schema generation.")]
	private static void AddValidation(IServiceCollection services)
	{
		services.AddJsonSchemaValidation();
	}
}