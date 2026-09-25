using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies the documentation page the test host publishes.
/// </summary>
public class PagePublicationTests
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
	public async Task PageIsServedAsHtml()
	{
		var response = await _client.GetAsync("/openapi/reference");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
	}

	[Test]
	public async Task PageCarriesItsTemplates()
	{
		var body = await _client.GetStringAsync("/openapi/reference");

		Assert.That(body, Does.Contain("oa-shell"), "markup");
		Assert.That(body, Does.Contain("--oa-accent"), "styles");
		Assert.That(body, Does.Contain("collectOperations"), "script");
	}

	[Test]
	public async Task PageIsToldWhereTheDocumentIs()
	{
		var body = await _client.GetStringAsync("/openapi/reference");

		Assert.That(body, Does.Contain("\"documentUrl\":\"/openapi.json\""));
	}

	[Test]
	public async Task PageUsesTheDocumentTitle()
	{
		var body = await _client.GetStringAsync("/openapi/reference");

		Assert.That(body, Does.Contain("<title>testhost</title>"));
	}

	[Test]
	public async Task RegisteringThePagePublishesTheDocument()
	{
		var response = await _client.GetAsync("/openapi.json");

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
	}

	[Test]
	public async Task NestedDocumentOptionsAreApplied()
	{
		var document = await _client.GetFromJsonAsync<JsonObject>("/openapi.json");

		Assert.That(document!["info"]!["version"]!.GetValue<string>(), Is.EqualTo("2.4.0"));
	}
}
