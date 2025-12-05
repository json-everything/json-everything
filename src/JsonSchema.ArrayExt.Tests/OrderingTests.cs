using System.Text.Json;
using Json.Pointer;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.ArrayExt.Tests;

public class OrderingTests
{
	[Test]
	public void NumberDirectionAscending_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = JsonDocument.Parse("[2, 3, 4, 5]").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionAscending_Duplicate_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = JsonDocument.Parse("[2, 3, 3, 5]").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionAscending_Failing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = JsonDocument.Parse("[2, 3, 1, 5]").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void NumberDirectionDescending_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = JsonDocument.Parse("[5, 4, 3, 2]").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionDescending_Duplicate_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = JsonDocument.Parse("[5, 4, 4, 2]").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void NumberDirectionDescending_Failing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = JsonDocument.Parse("[5, 4, 6, 2]").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void StringDirectionAscending_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = JsonDocument.Parse("""["alpha", "beta", "charlie", "delta"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionAscending_Duplicate_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = JsonDocument.Parse("""["alpha", "beta", "beta", "delta"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionAscending_Failing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Ascending));

		var instance = JsonDocument.Parse("""["alpha", "charlie", "beta", "delta"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void StringDirectionDescending_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = JsonDocument.Parse("""["delta", "charlie", "beta", "alpha"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionDescending_Duplicate_Passing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = JsonDocument.Parse("""["delta", "charlie", "charlie", "alpha"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void StringDirectionDescending_Failing()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, Direction.Descending));

		var instance = JsonDocument.Parse("""["delta", "beta", "charlie", "alpha"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void IgnoreCaseFalse_Passing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, ignoreCase: false));

		var instance = JsonDocument.Parse("""["alpha", "beta", "charlie", "delta"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}

	[Test]
	public void IgnoreCaseFalse_Failing()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, ignoreCase: false));

		var instance = JsonDocument.Parse("""["alpha", "Beta", "charlie", "delta"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertInvalid();
	}

	[Test]
	public void IgnoreCaseTrue()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.ArrayExt_202012Id)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
			.Ordering(new OrderingSpecifier(JsonPointer.Empty, ignoreCase: true));

		var instance = JsonDocument.Parse("""["alpha", "Beta", "charlie", "delta"]""").RootElement;

		var result = schema.Evaluate(instance);

		result.AssertValid();
	}
}