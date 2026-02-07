using System;
using System.Text.Json;
using Json.Schema.Generation;
using Json.Schema.Generation.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Json.Schema.Api;

/// <summary>
/// Provides extension methods for configuring JSON Schema validation in ASP.NET Core MVC applications.
/// </summary>
/// <remarks>The extension methods in this class enable integration of JSON Schema-based validation into the MVC
/// request pipeline. These methods are intended to be used during application startup to add model validation based on
/// JSON Schema definitions. All methods are static and designed for use with dependency injection and MVC configuration
/// patterns.</remarks>
public static class Extensions
{
	/// <summary>
	/// Enables JSON Schema-based validation for MVC controllers by adding the necessary filters and model binders to the
	/// application's MVC pipeline.
	/// </summary>
	/// <remarks>This method registers a filter that validates incoming JSON request bodies against their associated
	/// JSON Schemas and configures the JSON serializer to support schema-based validation. Use this method to enforce
	/// schema compliance for models in your MVC application.</remarks>
	/// <param name="builder">The MVC builder to configure. Cannot be null.</param>
	/// <param name="configure">An optional delegate to configure the generative JSON schema validation converter. If null, default settings are
	/// used.</param>
	/// <returns>The same <see cref="IMvcBuilder"/> instance so that additional configuration calls can be chained.</returns>
	public static IMvcBuilder AddJsonSchemaValidation(this IMvcBuilder builder, Action<GenerativeValidatingJsonConverter>? configure = null)
	{
		builder.Services.Configure<MvcOptions>(options =>
		{
			options.Filters.Add<JsonSchemaValidationFilter>();
			options.ModelBinderProviders.Insert(0, new ValidatingJsonModelBinderProvider());
		});

		builder.AddJsonOptions(opt =>
		{
			opt.JsonSerializerOptions.AddJsonSchemaValidation(configure);
		});

		return builder;
	}

	private static JsonSerializerOptions AddJsonSchemaValidation(this JsonSerializerOptions options, Action<GenerativeValidatingJsonConverter>? configure = null)
	{
		var converter = new GenerativeValidatingJsonConverter();

		if (configure is null)
		{
			converter.GeneratorConfiguration.PropertyNameResolver = PropertyNameResolvers.CamelCase;
			converter.EvaluationOptions.OutputFormat = OutputFormat.Hierarchical;
			converter.EvaluationOptions.RequireFormatValidation = true;
		}
		else
		{
			configure(converter);
		}

		options.Converters.Add(converter);

		return options;
	}
}