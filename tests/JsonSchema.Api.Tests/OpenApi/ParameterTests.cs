using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies how parameters and group prefixes are described.
/// </summary>
public class ParameterTests
{
	private ApiTestFixture _fixture = null!;
	private HttpClient _client = null!;
	private JsonObject _document = null!;

	[SetUp]
	public async Task SetUp()
	{
		_fixture = new ApiTestFixture();
		_client = _fixture.Client;
		_document = (await _client.GetFromJsonAsync<JsonObject>("/openapi.json"))!;
	}

	[TearDown]
	public void TearDown()
	{
		_fixture.Dispose();
	}

	private JsonObject Parameter(string path, string name) =>
		_document["paths"]![path]!["get"]!["parameters"]!.AsArray()
			.Single(x => x!["name"]!.GetValue<string>() == name)!.AsObject();

	[Test]
	public void InjectedServiceAfterBodyDoesNotDisplaceIt()
	{
		var operation = _document["paths"]!["/minimal/test/injected"]!["post"]!;

		var schema = operation["requestBody"]!["content"]!["application/json"]!["schema"]!;
		Assert.That(schema["$ref"]!.GetValue<string>(), Is.EqualTo("#/components/schemas/SimpleModel"));
		Assert.That(operation["parameters"], Is.Null);
	}

	[Test]
	public void InjectedServiceOnGetIsNotABody()
	{
		var operation = _document["paths"]!["/minimal/test/injected/{id}"]!["get"]!.AsObject();

		Assert.That(operation.ContainsKey("requestBody"), Is.False);
		Assert.That(operation["parameters"]!.AsArray().Select(x => x!["name"]!.GetValue<string>()), Is.EqualTo(new[] { "id" }));
	}

	[Test]
	public void ChainedGroupKeepsItsPrefix()
	{
		Assert.That(_document["paths"]!.AsObject().ContainsKey("/minimal/chained/{id}"), Is.True);
		Assert.That(_document["paths"]!.AsObject().ContainsKey("/{id}"), Is.False);
	}

	[TestCase("/api/test/filter")]
	[TestCase("/minimal/test/filter")]
	public void NullableQueryParametersAreOptional(string path)
	{
		Assert.That(Parameter(path, "category")["required"]!.GetValue<bool>(), Is.False);
		Assert.That(Parameter(path, "customerId")["required"]!.GetValue<bool>(), Is.False);
		Assert.That(Parameter(path, "page")["required"]!.GetValue<bool>(), Is.False);
	}

	[TestCase("/api/test/filter")]
	[TestCase("/minimal/test/filter")]
	public void NullableQueryParametersCarrySchemas(string path)
	{
		var category = Parameter(path, "category")["schema"]!["enum"]!.AsArray().Select(x => x!.GetValue<string>());
		Assert.That(category, Is.EqualTo(new[] { "Books", "Toys" }));

		var customerId = Parameter(path, "customerId")["schema"]!;
		Assert.That(customerId["type"]!.GetValue<string>(), Is.EqualTo("string"));
		Assert.That(customerId["format"]!.GetValue<string>(), Is.EqualTo("uuid"));
	}
}
