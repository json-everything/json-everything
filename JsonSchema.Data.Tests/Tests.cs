using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema.Tests;
using NUnit.Framework;

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
	public void Setup()
	{
		Vocabularies.Register();

		ValidationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}

	[Test]
	public void InstanceRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRef.Validate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRef.Validate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRelativeRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":{\"bar\":10}}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRelativeRef.Validate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRelativeRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":{\"bar\":10}}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = InstanceRelativeRef.Validate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRef_InvalidValueType()
	{
		var instanceData = "{\"minValue\":true,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.Throws<JsonException>(() => InstanceRef.Validate(instance));
	}

	[Test]
	public void InstanceRef_Unresolvable()
	{
		var instanceData = "{\"minValu\":5,\"foo\":10}";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.Throws<RefResolutionException>(() => InstanceRef.Validate(instance));
	}

	[Test]
	public void ExternalRef_Passing()
	{
		try
		{
			DataKeyword.Fetch = _ => JsonNode.Parse("{\"minValue\":5}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = ExternalRef.Validate(instance);

			result.AssertValid();
		}
		finally
		{
			DataKeyword.Fetch = null!;
		}
	}

	[Test]
	public void ExternalRef_Failing()
	{
		try
		{
			DataKeyword.Fetch = _ => JsonNode.Parse("{\"minValue\":15}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = ExternalRef.Validate(instance);

			result.AssertInvalid();
		}
		finally
		{
			DataKeyword.Fetch = null!;
		}
	}
}