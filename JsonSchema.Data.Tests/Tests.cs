using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests;

public class Tests
{
	private static readonly JsonSchema _instanceRef = new JsonSchemaBuilder()
		.Schema(MetaSchemas.Data_202012Id)
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Data(("minimum", "/minValue"))
			)
		);

	private static readonly JsonSchema _instanceRefOptional = new JsonSchemaBuilder()
		.Schema(MetaSchemas.Data_202012Id)
		.Type(SchemaValueType.Object)
		.Properties(
			("foo", new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.OptionalData(("minimum", "/minValue"))
			)
		);

	private static readonly JsonSchema _instanceRelativeRef = new JsonSchemaBuilder()
		.Schema(MetaSchemas.Data_202012Id)
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

	private static readonly JsonSchema _externalRef = new JsonSchemaBuilder()
		.Schema(MetaSchemas.Data_202012Id)
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
		var instance = JsonNode.Parse(instanceData);

		var result = _instanceRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":10}";
		var instance = JsonNode.Parse(instanceData);

		var result = _instanceRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRelativeRef_Passing()
	{
		var instanceData = "{\"minValue\":5,\"foo\":{\"bar\":10}}";
		var instance = JsonNode.Parse(instanceData);

		var result = _instanceRelativeRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRelativeRef_Failing()
	{
		var instanceData = "{\"minValue\":15,\"foo\":{\"bar\":10}}";
		var instance = JsonNode.Parse(instanceData);

		var result = _instanceRelativeRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRef_InvalidValueType()
	{
		var instanceData = "{\"minValue\":true,\"foo\":10}";
		var instance = JsonNode.Parse(instanceData);

		Assert.Throws<JsonException>(() => _instanceRef.Evaluate(instance));
	}

	[Test]
	public void InstanceRef_Unresolvable()
	{
		var instanceData = "{\"minValu\":5,\"foo\":10}";
		var instance = JsonNode.Parse(instanceData);

		Assert.Throws<RefResolutionException>(() => _instanceRef.Evaluate(instance));
	}

	[Test]
	public void InstanceRefOptional_Unresolvable()
	{
		var instanceData = "{\"minValu\":5,\"foo\":10}";
		var instance = JsonNode.Parse(instanceData);

		var result = _instanceRefOptional.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void ExternalRef_Passing()
	{
		try
		{
			DataKeyword.Fetch = _ => JsonNode.Parse("{\"minValue\":5}");

			var instanceData = "{\"foo\":10}";
			var instance = JsonNode.Parse(instanceData);

			var result = _externalRef.Evaluate(instance);

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
			var instance = JsonNode.Parse(instanceData);

			var result = _externalRef.Evaluate(instance);

			result.AssertInvalid();
		}
		finally
		{
			DataKeyword.Fetch = null!;
		}
	}

	private static readonly JsonSchema _pathRef = new JsonSchemaBuilder()
		.Schema(MetaSchemas.Data_202012Id)
		.Type(SchemaValueType.Object)
		.Properties(
			("options", new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Properties(
						("id", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
						("value", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
					.Required("id", "value")
				)
			),
			("selected", new JsonSchemaBuilder()
				.Data(
					(EnumKeyword.Name, "$.options[*].id")
				)
			)
		);

	[Test]
	public void SpecExamplePassing()
	{
		var instance = JsonNode.Parse(@"{
  ""options"": [
    { ""id"": 1, ""value"": ""foo"" },
    { ""id"": 2, ""value"": ""bar"" },
    { ""id"": 3, ""value"": ""baz"" },
    { ""id"": 4, ""value"": ""quux"" }
  ],
  ""selection"": 2
}");

		var result = _pathRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void SpecExampleFailing()
	{
		var instance = JsonNode.Parse(@"{
  ""options"": [
    { ""id"": 1, ""value"": ""foo"" },
    { ""id"": 2, ""value"": ""bar"" },
    { ""id"": 3, ""value"": ""baz"" },
    { ""id"": 4, ""value"": ""quux"" }
  ],
  ""selection"": 42
}");

		var result = _pathRef.Evaluate(instance);

		result.AssertValid();
	}
}