using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Provides extensions on various OpenAPI types.
/// </summary>
public static class OpenApiExtensions
{
	/// <summary>
	/// Validates a payload against the schema at the indicated location.
	/// </summary>
	/// <param name="openApiDocument">The OpenAPI document.</param>
	/// <param name="payload">The payload to validate.</param>
	/// <param name="schemaLocation">The location within the document where the schema can be found.</param>
	/// <param name="options">(optional) The evaluation options.  This should be the same options object used to initialize the OpenAPI document.</param>
	/// <returns>The evaluation options if the schema was found; otherwise null.</returns>
	public static EvaluationResults? EvaluatePayload(this OpenApiDocument openApiDocument, JsonNode? payload, JsonPointer schemaLocation, EvaluationOptions? options = null)
	{
		var schema = openApiDocument.Find<JsonSchema>(schemaLocation);
		if (schema is null) return null;

		using var doc = JsonDocument.Parse(payload?.ToJsonString() ?? "null");

		return schema.Evaluate(doc.RootElement, options);
	}

	/// <summary>
	/// Validates a payload against the schema at the indicated location.
	/// </summary>
	/// <param name="openApiDocument">The OpenAPI document.</param>
	/// <param name="payload">The payload to validate.</param>
	/// <param name="schemaLocation">The location within the document where the schema can be found.</param>
	/// <param name="options">(optional) The evaluation options.  This should be the same options object used to initialize the OpenAPI document.</param>
	/// <returns>The evaluation options if the schema was found; otherwise null.</returns>
	public static EvaluationResults? EvaluatePayload(this OpenApiDocument openApiDocument, JsonDocument payload, JsonPointer schemaLocation, EvaluationOptions? options = null)
	{
		var schema = openApiDocument.Find<JsonSchema>(schemaLocation);

		return schema?.Evaluate(payload.RootElement, options);
	}

	/// <summary>
	/// Validates a payload against the schema at the indicated location.
	/// </summary>
	/// <param name="openApiDocument">The OpenAPI document.</param>
	/// <param name="payload">The payload to validate.</param>
	/// <param name="schemaLocation">The location within the document where the schema can be found.</param>
	/// <param name="options">(optional) The evaluation options.  This should be the same options object used to initialize the OpenAPI document.</param>
	/// <returns>The evaluation options if the schema was found; otherwise null.</returns>
	public static EvaluationResults? EvaluatePayload(this OpenApiDocument openApiDocument, JsonElement payload, JsonPointer schemaLocation, EvaluationOptions? options = null)
	{
		var schema = openApiDocument.Find<JsonSchema>(schemaLocation);

		return schema?.Evaluate(payload, options);
	}
}