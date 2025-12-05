using System.Text.Json;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests;

public class Tests
{
	private static readonly JsonSchema _instanceRef =
		new JsonSchemaBuilder()
			.Schema(Dialect.Data_202012Id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Data(("minimum", "/minValue"))
				)
			);

	private static readonly JsonSchema _instanceRefOptional =
		new JsonSchemaBuilder()
			.Schema(Dialect.Data_202012Id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.OptionalData(("minimum", "/minValue"))
				)
			);

	private static readonly JsonSchema _instanceRelativeRef =
		new JsonSchemaBuilder()
			.Schema(Dialect.Data_202012Id)
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

	[Test]
	public void InstanceRef_Passing()
	{
		var instanceData = """{"minValue":5,"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = _instanceRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void InstanceRef_Failing()
	{
		var instanceData = """{"minValue":15,"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = _instanceRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	[Ignore("Relative pointer unsupported")]
	public void InstanceRelativeRef_Passing()
	{
		var instanceData = """{"minValue":5,"foo":{"bar":10}}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = _instanceRelativeRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	[Ignore("Relative pointer unsupported")]
	public void InstanceRelativeRef_Failing()
	{
		var instanceData = """{"minValue":15,"foo":{"bar":10}}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = _instanceRelativeRef.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void InstanceRef_InvalidValueType()
	{
		var instanceData = """{"minValue":true,"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.Throws<JsonSchemaException>(() => _instanceRef.Evaluate(instance));
	}

	[Test]
	public void InstanceRef_Unresolvable()
	{
		var instanceData = """{"minValu":5,"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		Assert.Throws<DataRefResolutionException>(() => _instanceRef.Evaluate(instance));
	}

	[Test]
	public void InstanceRefOptional_Unresolvable()
	{
		var instanceData = """{"minValu":5,"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = _instanceRefOptional.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void ExternalRef_Passing()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Data_202012
		};
		MetaSchemas.Register(buildOptions);
		buildOptions.GetDataRegistry().Fetch = _ => JsonDocument.Parse("""{"minValue":5}""").RootElement;

		JsonSchema externalRef =
			new JsonSchemaBuilder(buildOptions)
				.Schema(Dialect.Data_202012Id)
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Data(("minimum", "http://json.test/data#/minValue"))
					)
				);

		var instanceData = """{"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = externalRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void ExternalRef_Failing()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Data_202012
		};
		MetaSchemas.Register(buildOptions);
		buildOptions.GetDataRegistry().Fetch = _ => JsonDocument.Parse("""{"minValue":15}""").RootElement;

		JsonSchema externalRef =
			new JsonSchemaBuilder(buildOptions)
				.Schema(Dialect.Data_202012Id)
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Data(("minimum", "http://json.test/data#/minValue"))
					)
				);

		var instanceData = """{"foo":10}""";
		var instance = JsonDocument.Parse(instanceData).RootElement;

		var result = externalRef.Evaluate(instance);

		result.AssertInvalid();
	}

	private static readonly JsonSchema _pathRef = new JsonSchemaBuilder()
		.Schema(Dialect.Data_202012Id)
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
					("enum", "$.options[*].id")
				)
			)
		);

	[Test]
	public void SpecExamplePassing()
	{
		var instance = JsonDocument.Parse(
			"""
			{
			  "options": [
			    { "id": 1, "value": "foo" },
			    { "id": 2, "value": "bar" },
			    { "id": 3, "value": "baz" },
			    { "id": 4, "value": "quux" }
			  ],
			  "selection": 2
			}
			""").RootElement;

		var result = _pathRef.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void SpecExampleFailing()
	{
		var instance = JsonDocument.Parse(
			"""
			{
			  "options": [
			    { "id": 1, "value": "foo" },
			    { "id": 2, "value": "bar" },
			    { "id": 3, "value": "baz" },
			    { "id": 4, "value": "quux" }
			  ],
			  "selection": 42
			}
			""").RootElement;

		var result = _pathRef.Evaluate(instance);

		result.AssertValid();
	}
}