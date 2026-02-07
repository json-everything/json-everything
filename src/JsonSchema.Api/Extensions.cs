using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Json.Schema.Generation;
using Json.Schema.Generation.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Json.Schema.Api;

public static class Extensions
{
	[RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
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