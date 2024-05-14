using System.Text.Json.Nodes;
using Json.Pointer;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.ArrayExt.Tests;

public class OrderingTests
{
	[Test]
	public void NumberDirectionAscending_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = new JsonArray(2, 3, 4, 5);

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionAscending_Duplicate_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = new JsonArray(2, 3, 3, 5);

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionAscending_Failing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = new JsonArray(2, 3, 1, 5);

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void NumberDirectionDescending_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = new JsonArray(5, 4, 3, 2);

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionDescending_Duplicate_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = new JsonArray(5, 4, 4, 2);

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionDescending_Failing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = new JsonArray(5, 4, 6, 2);

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void StringDirectionAscending_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = new JsonArray("alpha", "beta", "charlie", "delta");

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionAscending_Duplicate_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = new JsonArray("alpha", "beta", "beta", "delta");

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionAscending_Failing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = new JsonArray("alpha", "charlie", "beta", "delta");

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void StringDirectionDescending_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = new JsonArray("delta", "charlie", "beta", "alpha");

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionDescending_Duplicate_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = new JsonArray("delta", "charlie", "charlie", "alpha");

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionDescending_Failing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = new JsonArray("delta", "beta", "charlie", "alpha");

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void IgnoreCaseFalse_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, ignoreCase: false));

		var instance = new JsonArray("alpha", "beta", "charlie", "delta");

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void IgnoreCaseFalse_Failing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, ignoreCase: false));

		var instance = new JsonArray("alpha", "Beta", "charlie", "delta");

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void IgnoreCaseTrue()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, ignoreCase: true));

		var instance = new JsonArray("alpha", "Beta", "charlie", "delta");

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}
}