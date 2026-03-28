#if NET9_0_OR_GREATER

using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Schema.Api.Tests.TestHost;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Api.Tests;

public class OpenApiSchemaComparisonTests
{
	[Test]
	public async Task OpenApiSchemas_MatchGeneratedSchemas()
	{
		using var fixture = new ApiTestFixture();
		using var client = fixture.Client;

		var response = await client.GetAsync("/openapi/v1.json");
		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

		var openApi = JsonNode.Parse(await response.Content.ReadAsStringAsync()) as JsonObject;
		Assert.That(openApi, Is.Not.Null);

		var schemas = openApi!["components"]?["schemas"] as JsonObject;
		Assert.That(schemas, Is.Not.Null);

		AssertSchemaEquivalent(schemas!, nameof(SimpleModel), GeneratedJsonSchemas.SimpleModel);
		AssertSchemaEquivalent(schemas!, nameof(StrictModel), GeneratedJsonSchemas.StrictModel);
		AssertSchemaEquivalent(schemas!, nameof(MultiWordModel), GeneratedJsonSchemas.MultiWordModel);
	}

	private static void AssertSchemaEquivalent(JsonObject openApiSchemas, string schemaName, JsonSchema generatedSchema)
	{
		var openApiSchema = openApiSchemas[schemaName] as JsonObject;
		Assert.That(openApiSchema, Is.Not.Null, $"OpenAPI schema '{schemaName}' was not found.");

		var generated = JsonNode.Parse(generatedSchema.Root.Source.GetRawText()) as JsonObject;
		Assert.That(generated, Is.Not.Null, $"Generated schema for '{schemaName}' was null.");

		generated!.Remove("$schema");

		JsonAssert.AreEquivalent(generated, openApiSchema!);
	}
}

#endif
