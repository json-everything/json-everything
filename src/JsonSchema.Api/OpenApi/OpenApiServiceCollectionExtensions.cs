using System;
using System.Linq;
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
		Action<OpenApiOptions>? configure = null) =>
		Add(services, null, configure);

	/// <summary>
	/// Describes the endpoints placed in a named description, and publishes it.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="name">
	/// The description's name, matching a name given to
	/// <see cref="OpenApiDocumentAttribute"/> on a controller.  To configure the default
	/// description — the one holding every controller without that attribute — use the
	/// overload that takes no name.
	/// </param>
	/// <param name="configure">
	/// An optional delegate that configures the description and how it is published.
	/// </param>
	/// <returns>The same <see cref="IServiceCollection"/>, so calls can be chained.</returns>
	/// <remarks>
	/// Call this once per description.  A named description defaults to serving under its
	/// own name — `/openapi/admin.json` and `/openapi/admin/reference` — so several can be
	/// published without configuring paths.
	/// </remarks>
	/// <example>
	/// <code>
	/// builder.Services.AddOpenApi();
	/// builder.Services.AddOpenApi("admin", c =>
	/// {
	///     c.Document.Info.Title = "Admin API";
	/// });
	/// </code>
	/// </example>
	public static IServiceCollection AddOpenApi(
		this IServiceCollection services,
		string name,
		Action<OpenApiOptions>? configure = null)
	{
		if (name is null)
			throw new ArgumentNullException(nameof(name),
				"Use the overload that takes no name to configure the default description.");

		return Add(services, name, configure);
	}

	private static IServiceCollection Add(
		IServiceCollection services,
		string? name,
		Action<OpenApiOptions>? configure)
	{
		var document = OpenApiDocumentBuilder.Build(OpenApiDocumentBuilder.CollectFragments(name));
		var options = new OpenApiOptions(document, name);

		configure?.Invoke(options);
		options.Validate();

		if (options.AddValidation)
			AddValidation(services);

		// Each description is its own registration, but a second call naming the same
		// description replaces the first — how a test host or an environment-specific
		// configuration reconfigures one without publishing it twice.
		var existing = services.FirstOrDefault(x =>
			x.ServiceType == typeof(OpenApiOptions) &&
			x.ImplementationInstance is OpenApiOptions o &&
			o.Name == name);

		if (existing is not null)
			services.Remove(existing);

		// Checked after the replacement is removed, so reconfiguring a description does not
		// collide with the registration it replaces.
		VerifyNoCollisions(services, options);

		services.AddSingleton(options);

		// Only the default description is resolvable as a bare `OpenApiDocument`, since that
		// resolution has no way to say which one it wants.
		if (name is null)
		{
			services.RemoveAll<OpenApiDocument>();
			services.AddSingleton(options.Document);
		}

		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<IStartupFilter, OpenApiStartupFilter>());

		return services;
	}

	/// <summary>
	/// Rejects a description that would collide with one already registered.
	/// </summary>
	/// <remarks>
	/// A collision is silent otherwise: two descriptions sharing a served path leave one
	/// unreachable, and two sharing a file path leave whichever is written last on disk.
	/// </remarks>
	private static void VerifyNoCollisions(IServiceCollection services, OpenApiOptions options)
	{
		var registered = services
			.Select(x => x.ImplementationInstance)
			.OfType<OpenApiOptions>()
			.ToArray();

		foreach (var other in registered)
		{
			Reject(options.DocumentPath, other.DocumentPath, nameof(OpenApiOptions.DocumentPath), other);
			Reject(options.InteractivePath, other.InteractivePath, nameof(OpenApiOptions.InteractivePath), other);
			Reject(options.FileOutputPath, other.FileOutputPath, nameof(OpenApiOptions.FileOutputPath), other);

			// A served path and a page path collide with each other just as readily.
			Reject(options.DocumentPath, other.InteractivePath, nameof(OpenApiOptions.DocumentPath), other);
			Reject(options.InteractivePath, other.DocumentPath, nameof(OpenApiOptions.InteractivePath), other);
		}

		void Reject(string? mine, string? theirs, string property, OpenApiOptions other)
		{
			if (mine is null || theirs is null) return;
			if (!string.Equals(mine, theirs, StringComparison.OrdinalIgnoreCase)) return;

			throw new InvalidOperationException(
				$"`{property}` is `{mine}` on the {Describe(options)}, which the " +
				$"{Describe(other)} already uses.  Each description needs its own paths.");
		}
	}

	private static string Describe(OpenApiOptions options) =>
		options.Name is null ? "default description" : $"`{options.Name}` description";

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Reached only when the consumer leaves `AddValidation` on, which opts into runtime schema generation.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Reached only when the consumer leaves `AddValidation` on, which opts into runtime schema generation.")]
	private static void AddValidation(IServiceCollection services)
	{
		services.AddJsonSchemaValidation();
	}
}