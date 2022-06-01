using System.Collections.Generic;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class AdditionalItemsTests
{
	[AdditionalItems(true)]
	private class AdditionalItemsTrue : List<int>
	{
	}

	[Test]
	public void TrueIsAdded()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.AdditionalItems(true);

		var actual = new JsonSchemaBuilder().FromType<AdditionalItemsTrue>();

		AssertEqual(expected, actual);
	}

	[AdditionalItems(false)]
	private class AdditionalItemsFalse : List<int>
	{
	}

	[Test]
	public void FalseIsAdded()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.AdditionalItems(false);

		var actual = new JsonSchemaBuilder().FromType<AdditionalItemsFalse>();

		AssertEqual(expected, actual);
	}

	[AdditionalItems(typeof(string))]
	private class AdditionalItemsString : List<int>
	{
	}

	[Test]
	public void TypeIsAdded()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.AdditionalItems(new JsonSchemaBuilder().Type(SchemaValueType.String));

		var actual = new JsonSchemaBuilder().FromType<AdditionalItemsString>();

		AssertEqual(expected, actual);
	}

	private class PropClass : List<int>
	{
		public string Bar { get; set; }
	}

	private class AdditionalItemsOnProp
	{
		[AdditionalItems(false)]
		public PropClass Foo { get; set; }
	}

	[Test]
	public void FalseIsAddedToProp()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
					.AdditionalItems(false)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<AdditionalItemsOnProp>();

		AssertEqual(expected, actual);
	}
}