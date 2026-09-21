using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies the OpenAPI description the test host publishes.
/// </summary>
public class DocumentPublicationTests
{
	private ApiTestFixture _fixture = null!;
	private HttpClient _client = null!;

	[SetUp]
	public void SetUp()
	{
		_fixture = new ApiTestFixture();
		_client = _fixture.Client;
	}

	[TearDown]
	public void TearDown()
	{
		_fixture.Dispose();
	}

	[Test]
	public async Task DocumentIsServedAsJson()
	{
		var response = await _client.GetAsync("/openapi.json");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

		var document = await response.Content.ReadFromJsonAsync<JsonObject>();

		TestConsole.WriteLine(document.AsJsonString(new() { WriteIndented = true }));

		Assert.That(document["openapi"]!.GetValue<string>(), Is.EqualTo("3.1.1"));
	}

	[Test]
	public async Task DocumentIsServedAsYaml()
	{
		var response = await _client.GetAsync("/openapi.yaml");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/yaml"));

		var body = await response.Content.ReadAsStringAsync();

		TestConsole.WriteLine(body);

		Assert.That(body, Does.Contain("openapi:"));
	}

	[Test]
	public async Task ExtensionlessRouteHonorsAcceptHeader()
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, "/openapi");
		request.Headers.Add("Accept", "application/yaml");

		var response = await _client.SendAsync(request);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/yaml"));
	}

	[Test]
	public async Task ExtensionlessRouteDefaultsToJson()
	{
		var response = await _client.GetAsync("/openapi");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
	}

	[Test]
	public async Task ConfigurationDelegateIsApplied()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");

		Assert.That(
			document!["info"]!["description"]!.GetValue<string>(),
			Is.EqualTo("Test host for OpenAPI generation."));
	}

	[Test]
	public async Task ControllerAndMinimalApiRoutesAreBothPresent()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");
		var paths = document!["paths"]!.AsObject();

		Assert.That(paths.ContainsKey("/api/test/simple"), Is.True, "controller route");
		Assert.That(paths.ContainsKey("/minimal/test/simple"), Is.True, "minimal API route");
	}

	[Test]
	public async Task OperationCarriesRequestBodySchemaRef()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");

		var schema = document!["paths"]!["/api/test/simple"]!["post"]!["requestBody"]!
			["content"]!["application/json"]!["schema"]!;

		Assert.That(schema["$ref"]!.GetValue<string>(), Is.EqualTo("#/components/schemas/SimpleModel"));
	}

	[Test]
	public async Task OperationCarriesResponseSchemaRef()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");

		var schema = document!["paths"]!["/minimal/test/simple"]!["post"]!["responses"]!["200"]!
			["content"]!["application/json"]!["schema"]!;

		Assert.That(schema["$ref"]!.GetValue<string>(), Is.EqualTo("#/components/schemas/SimpleModel"));
	}

	[Test]
	public async Task ValidatedOperationsDeclareTheValidationFailureResponse()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");

		var responses = document!["paths"]!["/api/test/simple"]!["post"]!["responses"]!.AsObject();

		Assert.That(responses.ContainsKey("400"), Is.True);
		Assert.That(
			responses["400"]!["content"]!["application/problem+json"], Is.Not.Null);
	}

	[Test]
	public async Task UnvalidatedOperationsDoNotDeclareIt()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");

		var responses = document!["paths"]!["/api/test/unvalidated"]!["post"]!["responses"]!.AsObject();

		Assert.That(responses.ContainsKey("400"), Is.False);
	}

	[Test]
	public async Task ComponentSchemasCarryNoIdOrSchemaKeyword()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");
		var schema = document!["components"]!["schemas"]!["SimpleModel"]!.AsObject();

		Assert.That(schema.ContainsKey("$id"), Is.False);
		Assert.That(schema.ContainsKey("$schema"), Is.False);
		Assert.That(schema["type"]!.GetValue<string>(), Is.EqualTo("object"));
	}
}
