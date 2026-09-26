using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Api.Tests;

/// <summary>
/// The generated schemas describe enums by name, so the default serializer has to accept names.
/// </summary>
public class EnumTests
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
	public async Task EnumName_PassesValidationAndBinding()
	{
		var content = new StringContent("""{"name": "Lego set", "category": "Toys"}""", Encoding.UTF8, "application/json");

		var response = await _client.PostAsync("/api/test/enum", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

		var body = await response.Content.ReadFromJsonAsync<JsonObject>();
		Assert.That(body!["category"]!.GetValue<string>(), Is.EqualTo("Toys"));
	}

	[Test]
	public async Task EnumNumber_FailsValidation()
	{
		var content = new StringContent("""{"name": "Lego set", "category": 1}""", Encoding.UTF8, "application/json");

		var response = await _client.PostAsync("/api/test/enum", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
		Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
	}
}
