using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Schema.Tests;
using NUnit.Framework;
#pragma warning disable CS1998

namespace Json.Schema.Data.Tests;

public class Tests
{
	private static JsonSchema InstanceRef { get; } = new JsonSchemaBuilder()
		.Schema("https://json-everything.net/meta/data-2022")
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Data(("minimum", "/minValue"))
			)
		);

	private static JsonSchema InstanceRelativeRef { get; } = new JsonSchemaBuilder()
		.Schema("https://json-everything.net/meta/data-2022")
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("bar", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Data(("minimum", "2/minValue"))
					)
				)
			)
		);

	private static JsonSchema ExternalRef { get; } = new JsonSchemaBuilder()
		.Schema("https://json-everything.net/meta/data-2022")
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Data(("minimum", "http://json.test/data#/minValue"))
			)
		);

	[OneTimeSetUp]
	public async Task Setup()
	{
		await Vocabularies.Register();

		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}

	[Test]
	public async Task InstanceRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = await InstanceRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public async Task InstanceRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = await InstanceRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public async Task InstanceRelativeRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":{\"bar\":10}}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = await InstanceRelativeRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public async Task InstanceRelativeRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":{\"bar\":10}}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = await InstanceRelativeRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRef_InvalidValueType()
	{
		var instanceData = "{\"minValue\":true,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.ThrowsAsync<JsonException>(() => InstanceRef.Evaluate(instance));
	}

	[Test]
	public void InstanceRef_Unresolvable()
	{
		var instanceData = "{\"minValu\":5,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.ThrowsAsync<RefResolutionException>(() => InstanceRef.Evaluate(instance));
	}

	[Test]
	public async Task ExternalRef_Passing()
	{
		try
		{
			DataKeyword.Fetch = async _ => JsonNode.Parse("{\"minValue\":5}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = await ExternalRef.Evaluate(instance);

			result.AssertValid();
		}
		finally
		{
			DataKeyword.Fetch = null!;
		}
	}

	[Test]
	public async Task ExternalRef_Failing()
	{
		try
		{
			DataKeyword.Fetch = async _ => JsonNode.Parse("{\"minValue\":15}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = await ExternalRef.Evaluate(instance);

			result.AssertInvalid();
		}
		finally
		{
			DataKeyword.Fetch = null!;
		}
	}
}