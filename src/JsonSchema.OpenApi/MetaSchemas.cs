using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Json.Schema.OpenApi;

/// <summary>
/// Provides access to the OpenAPI vocabulary meta-schemas and methods for registering its components.
/// </summary>
/// <remarks>Use this class to register and retrieve the meta-schemas required for working with the OpenAPI
/// vocabulary in JSON Schema processing. All members are static and intended for application-wide
/// configuration.</remarks>
public static class MetaSchemas
{
	/// <summary>
	/// The ID for the OpenAPI 3.1 dialect.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri OpenApi_31Id = new("https://spec.openapis.org/oas/3.1/dialect/base");

	/// <summary>
	/// The ID for the OpenAPI 3.1 vocabulary meta-schema.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri OpenApiMeta_31Id = new("https://spec.openapis.org/oas/3.1/meta/base");

	/// <summary>
	/// The ID for the OpenAPI 3.1 document schema.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri DocumentSchema_31Id = new("https://spec.openapis.org/oas/3.1/schema/2022-02-27");

	/// <summary>
	/// The OpenAPI 3.1 dialect meta-schema.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static JsonSchema OpenApiDialect_31 { get; private set; } = null!;

	/// <summary>
	/// The OpenAPI 3.1 vocabulary meta-schema.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static JsonSchema OpenApiMeta_31 { get; private set; } = null!;

	/// <summary>
	/// The OpenAPI 3.1 document schema.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static JsonSchema DocumentSchema_31 { get; private set; } = null!;

	/// <summary>
	/// Registers all components required to use the OpenAPI vocabulary.
	/// </summary>
	public static void Register(BuildOptions? buildOptions = null)
	{
		buildOptions ??= BuildOptions.Default;

		buildOptions.DialectRegistry.Register(Dialect.OpenApi_31);
		buildOptions.VocabularyRegistry.Register(Vocabulary.OpenApi_31);

		OpenApiDialect_31 = LoadMetaSchema("openapi-dialect-base", buildOptions);
		OpenApiMeta_31 = LoadMetaSchema("openapi-meta-base", buildOptions);
		DocumentSchema_31 = LoadMetaSchema("schema", buildOptions);
	}

	private static JsonSchema LoadMetaSchema(string resourceName, BuildOptions buildOptions)
	{
		var resources = typeof(MetaSchemas).Assembly.GetManifestResourceNames();
		var resourceStream = typeof(MetaSchemas).Assembly.GetManifestResourceStream(@$"Json.Schema.OpenApi.Meta_Schemas._3._1.{resourceName}.json");
		using var reader = new StreamReader(resourceStream!, Encoding.UTF8);

		var text = reader.ReadToEnd();
		var json = JsonDocument.Parse(text).RootElement;

		return JsonSchema.Build(json, buildOptions);
	}
}