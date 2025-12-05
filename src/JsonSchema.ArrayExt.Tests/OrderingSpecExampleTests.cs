using System.Text.Json;
using Json.Pointer;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.ArrayExt.Tests;

public class OrderingSpecExampleTests
{
	private static readonly JsonSchema _singleSpecifier =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Id("https://json-everything.test/single")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			)
			.Ordering(new OrderingSpecifier(JsonPointer.Parse("/foo")));

	private static readonly JsonSchema _multipleSpecifiers =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Id("https://json-everything.test/multiple")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			)
			.Ordering(
				new OrderingSpecifier(JsonPointer.Parse("/foo")),
				new OrderingSpecifier(JsonPointer.Parse("/bar"), Direction.Descending)
			);

	[Test]
	public void SingleSpecifierPassingInstance()
	{
		var instance = JsonDocument.Parse(
			"""
			[
			  { "foo": 1, "bar": "ipsum" },
			  { "foo": 1, "bar": "Lorem" },
			  { "foo": 2, "bar": "dolor" },
			  { "foo": 3, "bar": "sit" },
			  { "foo": 5, "bar": "amet" }
			]
			""").RootElement;

		var result = _singleSpecifier.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void SingleSpecifierFailingInstance()
	{
		var instance = JsonDocument.Parse(
			"""
			[
			  { "foo": 1, "bar": "Lorem" },
			  { "foo": 5, "bar": "amet" },
			  { "foo": 2, "bar": "dolor" },
			  { "foo": 1, "bar": "ipsum" },
			  { "foo": 3, "bar": "sit" }
			]
			""").RootElement;

		var result = _singleSpecifier.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void MultipleSpecifierPassingInstance()
	{
		// string is descending
		var instance = JsonDocument.Parse(
			"""
			[
			  { "foo": 1, "bar": "ipsum" },
			  { "foo": 1, "bar": "Lorem" },
			  { "foo": 2, "bar": "dolor" },
			  { "foo": 3, "bar": "sit" },
			  { "foo": 5, "bar": "amet" }
			]
			""").RootElement;

		var result = _multipleSpecifiers.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void MultipleSpecifierFailingInstance_Secondary()
	{
		// string is descending
		var instance = JsonDocument.Parse(
			"""
			[
			  { "foo": 1, "bar": "Lorem" },
			  { "foo": 1, "bar": "ipsum" },
			  { "foo": 2, "bar": "dolor" },
			  { "foo": 3, "bar": "sit" },
			  { "foo": 5, "bar": "amet" }
			]
			""").RootElement;

		var result = _multipleSpecifiers.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void MultipleSpecifierPassingInstance_WrongPriority()
	{
		var instance = JsonDocument.Parse(
			"""
			[
			  { "foo": 1, "bar": "Lorem" },
			  { "foo": 5, "bar": "amet" },
			  { "foo": 2, "bar": "dolor" },
			  { "foo": 1, "bar": "ipsum" },
			  { "foo": 3, "bar": "sit" }
			]
			""").RootElement;

		var result = _multipleSpecifiers.Evaluate(instance);

		result.AssertInvalid();
	}
}