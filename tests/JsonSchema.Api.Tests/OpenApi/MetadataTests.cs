using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies the descriptive metadata read from documentation comments, ASP.NET's metadata
/// attributes, and the fluent minimal-API calls.
/// </summary>
public class MetadataTests
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

	private JsonObject Operation(string path, string method) =>
		_document["paths"]![path]![method]!.AsObject();

	[Test]
	public void ControllerOperationIdIsQualifiedByController()
	{
		Assert.That(Operation("/api/Coverage/{id}", "get")["operationId"]!.GetValue<string>(), Is.EqualTo("Coverage_GetById"));
		Assert.That(Operation("/api/test/simple", "post")["operationId"]!.GetValue<string>(), Is.EqualTo("Test_PostSimple"));
	}

	[Test]
	public void OperationIdsAreUnique()
	{
		var ids = _document["paths"]!.AsObject()
			.SelectMany(path => path.Value!.AsObject())
			.Select(op => op.Value!["operationId"]?.GetValue<string>())
			.Where(id => id is not null)
			.ToList();

		Assert.That(ids, Is.Unique);
	}

	[Test]
	public void DocCommentSummaryAndRemarksReachTheOperation()
	{
		var operation = Operation("/api/Coverage/{id}", "get");

		Assert.That(operation["summary"]!.GetValue<string>(), Is.EqualTo("Fetches one item."));
		Assert.That(operation["description"]!.GetValue<string>(), Does.StartWith("The summary and this remark reach the operation; the param, returns, and response elements"));
	}

	[Test]
	public void DocCommentParamReachesTheParameter()
	{
		var parameter = Operation("/api/Coverage/{id}", "get")["parameters"]![0]!;

		Assert.That(parameter["name"]!.GetValue<string>(), Is.EqualTo("id"));
		Assert.That(parameter["description"]!.GetValue<string>(), Is.EqualTo("The item's identifier."));
	}

	[Test]
	public void DocCommentParamReachesTheRequestBody()
	{
		var body = Operation("/api/Coverage/{id}", "put")["requestBody"]!;

		Assert.That(body["description"]!.GetValue<string>(), Is.EqualTo("The item's new state."));
	}

	[Test]
	public void DocCommentReturnsDescribesTheSuccessResponse()
	{
		var responses = Operation("/api/Coverage/{id}", "get")["responses"]!;

		Assert.That(responses["200"]!["description"]!.GetValue<string>(), Is.EqualTo("The item."));
	}

	[Test]
	public void DocCommentResponseAddsAnUndeclaredResponse()
	{
		var responses = Operation("/api/Coverage/{id}", "get")["responses"]!;

		Assert.That(responses["404"]!["description"]!.GetValue<string>(), Is.EqualTo("No item has that identifier."));
	}

	[Test]
	public void AttributesWinOverTheDocComment()
	{
		var operation = Operation("/api/Coverage/search", "get");

		Assert.That(operation["summary"]!.GetValue<string>(), Is.EqualTo("Searches items by name."));
		Assert.That(operation["description"]!.GetValue<string>(), Is.EqualTo("Attributes win over the documentation comment."));
	}

	[Test]
	public void ControllerAndActionTagsCombine()
	{
		var tags = Operation("/api/Coverage/search", "get")["tags"]!.AsArray()
			.Select(x => x!.GetValue<string>());

		Assert.That(tags, Is.EqualTo(new[] { "Coverage", "Search" }));
	}

	[Test]
	public void ControllerTagsApplyToEveryAction()
	{
		var tags = Operation("/api/Coverage/{id}", "delete")["tags"]!.AsArray()
			.Select(x => x!.GetValue<string>());

		Assert.That(tags, Is.EqualTo(new[] { "Coverage" }));
	}

	[Test]
	public void FluentMetadataReachesMinimalApiOperations()
	{
		var operation = Operation("/minimal/test/search", "get");

		Assert.That(operation["summary"]!.GetValue<string>(), Is.EqualTo("Searches items by name."));
		Assert.That(operation["description"]!.GetValue<string>(), Is.EqualTo("Fluent metadata reaches the operation."));
		Assert.That(
			operation["tags"]!.AsArray().Select(x => x!.GetValue<string>()),
			Is.EqualTo(new[] { "Minimal", "Search" }));
	}

	[Test]
	public void MethodGroupHandlersCarryTheirDocCommentAndAttributes()
	{
		var operation = Operation("/minimal/test/methodgroup", "post");

		Assert.That(operation["summary"]!.GetValue<string>(), Is.EqualTo("Echoes the model."));
		Assert.That(operation["requestBody"]!["description"]!.GetValue<string>(), Is.EqualTo("The model to echo."));
		Assert.That(operation["responses"]!["200"]!["description"]!.GetValue<string>(), Is.EqualTo("The same model."));
		Assert.That(
			operation["tags"]!.AsArray().Select(x => x!.GetValue<string>()),
			Is.EqualTo(new[] { "MethodGroup" }));
	}

	[Test]
	public void UndocumentedOperationsCarryNoMetadata()
	{
		var operation = Operation("/minimal/test/simple", "post");

		Assert.That(operation.ContainsKey("summary"), Is.False);
		Assert.That(operation.ContainsKey("description"), Is.False);
		Assert.That(operation.ContainsKey("tags"), Is.False);
	}
}
