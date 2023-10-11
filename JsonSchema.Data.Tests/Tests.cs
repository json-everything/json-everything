using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests;

public class Tests
{
	private static JsonSchema InstanceRef { get; } = new JsonSchemaBuilder()
		.Schema("https://json-everything.net/meta/data-2023")
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Data(("minimum", "/minValue"))
			)
		);

	private static JsonSchema InstanceRelativeRef { get; } = new JsonSchemaBuilder()
		.Schema("https://json-everything.net/meta/data-2023")
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
		.Schema("https://json-everything.net/meta/data-2023")
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Data(("minimum", "http://json.test/data#/minValue"))
			)
		);

	[OneTimeSetUp]
	public void Setup()
	{
		Vocabularies.Register();

		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}

	[Test]
	public void InstanceRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRelativeRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":{\"bar\":10}}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRelativeRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRelativeRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":{\"bar\":10}}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRelativeRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRef_InvalidValueType()
	{
		var instanceData = "{\"minValue\":true,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.Throws<JsonException>(() => InstanceRef.Evaluate(instance));
	}

	[Test]
	public void InstanceRef_Unresolvable()
	{
		var instanceData = "{\"minValu\":5,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.Throws<RefResolutionException>(() => InstanceRef.Evaluate(instance));
	}

	[Test]
	public void ExternalRef_Passing()
	{
		try
		{
			ExternalDataRegistry.Fetch = _ => JsonNode.Parse("{\"minValue\":5}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = ExternalRef.Evaluate(instance);

			result.AssertValid();
		}
		finally
		{
			ExternalDataRegistry.Fetch = null!;
		}
	}

	[Test]
	public void ExternalRef_Failing()
	{
		try
		{
			ExternalDataRegistry.Fetch = _ => JsonNode.Parse("{\"minValue\":15}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = ExternalRef.Evaluate(instance);

			result.AssertInvalid();
		}
		finally
		{
			ExternalDataRegistry.Fetch = null!;
		}
	}
}