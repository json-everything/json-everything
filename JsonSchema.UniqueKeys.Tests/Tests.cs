using System.Text.Json;
using System.Threading.Tasks;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.UniqueKeys.Tests;

public class SpecExampleTests
{
	private readonly JsonSchema _singleKeySchema = new JsonSchemaBuilder()
		.Schema(MetaSchemas.UniqueKeysId)
		.Type(SchemaValueType.Array)
		.Items(new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			)
		)
		.UniqueKeys("/foo");
	private readonly JsonSchema _multiKeySchema = new JsonSchemaBuilder()
		.Schema(MetaSchemas.UniqueKeysId)
		.Type(SchemaValueType.Array)
		.Items(new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("var", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			)
		)
		.UniqueKeys("/foo", "/bar");

	[OneTimeSetUp]
	public static async Task Setup()
	{
		await Vocabularies.Register();

		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}

	[Test]
	public async Task SingleKeySchema_UniqueValuesAtKeyPasses()
	{
		var instance = JsonDocument.Parse(@"[
			  { ""foo"": 8 },
			  { ""foo"": 12 },
			  { ""foo"": 42 }
			]").RootElement;

		var results = await _singleKeySchema.Evaluate(instance);

		results.AssertValid();
	}

	[Test]
	public async Task SingleKeySchema_DuplicateValuesAtKeyFails()
	{
		var instance = JsonDocument.Parse(@"[
			  { ""foo"": 8 },
			  { ""foo"": 12 },
			  { ""foo"": 8 }
			]").RootElement;

		var results = await _singleKeySchema.Evaluate(instance);

		results.AssertInvalid();
	}

	[Test]
	public async Task SingleKeySchema_NotAllItemsHaveFooPasses()
	{
		var instance = JsonDocument.Parse(@"[
			  { ""foo"": 8 },
			  { ""bar"": 8 }
			]").RootElement;

		var results = await _singleKeySchema.Evaluate(instance);

		results.AssertValid();
	}

	[Test]
	public async Task SingleKeySchema_DuplicateValuesAtKeyFailsEvenThoughItemsAreDistinct()
	{
		var instance = JsonDocument.Parse(@"[
			  { ""foo"": 8, ""bar"": true },
			  { ""foo"": 12, ""bar"": true },
			  { ""foo"": 8, ""bar"": false }
			]").RootElement;

		var results = await _singleKeySchema.Evaluate(instance);

		results.AssertInvalid();
	}

	[Test]
	public async Task MultiKey_UniqueValuesAtKeysPasses()
	{
		var instance = JsonDocument.Parse(@"[
			  { ""foo"": 8, ""bar"": true },
			  { ""foo"": 12, ""bar"": true },
			  { ""foo"": 8, ""bar"": false }
			]").RootElement;

		var results = await _multiKeySchema.Evaluate(instance);

		results.AssertValid();
	}

	[Test]
	public async Task MultiKey_DuplicateValuesAtKeysFailsEvenThoughItemsAreDistinct()
	{
		var instance = JsonDocument.Parse(@"[
			  { ""foo"": 8, ""bar"": true, ""baz"": ""yes"" },
			  { ""foo"": 8, ""bar"": true, ""baz"": ""no"" },
			  { ""foo"": 8, ""bar"": false }
			]").RootElement;

		var results = await _multiKeySchema.Evaluate(instance);

		results.AssertInvalid();
	}
}