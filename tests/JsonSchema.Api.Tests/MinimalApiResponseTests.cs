using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Api.Tests;

public class MinimalApiResponseTests
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
	public async Task ValidRequest_Returns200WithBody()
	{
		var json = """{"name": "John Doe", "age": 30}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _client.PostAsync("/minimal/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

		var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
		Assert.That(responseBody, Is.Not.Null);
		Assert.That(responseBody!["name"]!.GetValue<string>(), Is.EqualTo("John Doe"));
		Assert.That(responseBody["age"]!.GetValue<int>(), Is.EqualTo(30));
	}

	[Test]
	public async Task MissingRequiredField_Returns400WithProblemDetails()
	{
		var json = """{"age": 25}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _client.PostAsync("/minimal/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));

		var problemDetails = await response.Content.ReadFromJsonAsync<JsonObject>();
		Assert.That(problemDetails, Is.Not.Null);
		Assert.That(problemDetails!["type"]!.GetValue<string>(), Is.EqualTo("https://json-everything.net/errors/validation"));
		Assert.That(problemDetails["title"]!.GetValue<string>(), Is.EqualTo("Validation Error"));
		Assert.That(problemDetails["status"]!.GetValue<int>(), Is.EqualTo(400));

		var errors = problemDetails["errors"] as JsonObject;
		Assert.That(errors, Is.Not.Null);
		Assert.That(errors!.Count, Is.GreaterThan(0));
	}

	[Test]
	public async Task AdditionalProperties_Returns400WithProblemDetails()
	{
		var json = """{"title": "Test", "active": true, "extraField": "not allowed"}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await _client.PostAsync("/minimal/test/strict", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));

		var problemDetails = await response.Content.ReadFromJsonAsync<JsonObject>();
		Assert.That(problemDetails, Is.Not.Null);

		Assert.That(problemDetails!["type"]!.GetValue<string>(), Is.EqualTo("https://json-everything.net/errors/validation"));
		Assert.That(problemDetails["title"]!.GetValue<string>(), Is.EqualTo("Validation Error"));
		Assert.That(problemDetails["status"]!.GetValue<int>(), Is.EqualTo(400));

		var errors = problemDetails["errors"] as JsonObject;
		Assert.That(errors, Is.Not.Null);
		Assert.That(errors!.Count, Is.GreaterThan(0));
	}
}
