using System;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests;

public class FlatSchemaTests
{
	[Test]
	public void GenerateNull()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Null);

		var result = schema.GenerateData();

		Assert.IsTrue(result.IsSuccess);
		Console.WriteLine(result.Result.ToJsonString());
		Assert.AreEqual(JsonValueKind.Null, result.Result.ValueKind);

		Assert.IsTrue(schema.Validate(result.Result).IsValid);
	}

	[Test]
	public void GenerateNumber()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10)
			.Maximum(20)
			.MultipleOf(0.2m);

		var result = schema.GenerateData();

		Assert.IsTrue(result.IsSuccess);
		Console.WriteLine(result.Result.ToJsonString());
		Assert.AreEqual(JsonValueKind.Number, result.Result.ValueKind);

		var value = result.Result.GetDecimal();
		Assert.IsTrue(value >= 10);
		Assert.IsTrue(value <= 20);
		Assert.IsTrue(value % 0.2m == 0);

		Assert.IsTrue(schema.Validate(result.Result).IsValid);
	}

	[Test]
	public void GenerateInteger()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.Minimum(10)
			.Maximum(20)
			.MultipleOf(3);

		var result = schema.GenerateData();

		Assert.IsTrue(result.IsSuccess);
		Console.WriteLine(result.Result.ToJsonString());
		Assert.AreEqual(JsonValueKind.Number, result.Result.ValueKind);

		var value = result.Result.GetDecimal();
		Assert.IsTrue(value >= 10);
		Assert.IsTrue(value <= 20);
		Assert.IsTrue(value % 3 == 0);
		Assert.IsTrue((int) value == value);

		Assert.IsTrue(schema.Validate(result.Result).IsValid);
	}

	[Test]
	public void GenerateIntegerWithDecimalMultipleOf()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Integer)
			.Minimum(10)
			.Maximum(20)
			.MultipleOf(0.3m);

		var result = schema.GenerateData();

		Assert.IsTrue(result.IsSuccess);
		Console.WriteLine(result.Result.ToJsonString());
		Assert.AreEqual(JsonValueKind.Number, result.Result.ValueKind);

		var value = result.Result.GetDecimal();
		Assert.IsTrue(value >= 10);
		Assert.IsTrue(value <= 20);
		Assert.IsTrue(value % 0.3m == 0);
		Assert.IsTrue((int) value == value);

		Assert.IsTrue(schema.Validate(result.Result).IsValid);
	}

	[Test]
	public void GenerateArrayOfNumbers()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Minimum(10)
				.Maximum(20))
			.MinItems(3)
			.MaxItems(10);

		var result = schema.GenerateData();

		Assert.IsTrue(result.IsSuccess);
		Console.WriteLine(result.Result.ToJsonString());
		Assert.AreEqual(JsonValueKind.Array, result.Result.ValueKind);

		var items = result.Result.EnumerateArray().ToArray();
		Assert.IsTrue(items.Length >= 3);
		Assert.IsTrue(items.Length <= 10);

		foreach (var item in items)
		{
			Assert.AreEqual(JsonValueKind.Number, item.ValueKind);

			var value = item.GetDecimal();
			Assert.IsTrue(value >= 10);
			Assert.IsTrue(value <= 20);
		}

		Assert.IsTrue(schema.Validate(result.Result).IsValid);
	}
}