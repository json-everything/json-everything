using System.Collections.Generic;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable ClassNeverInstantiated.Local

namespace Json.Schema.Generation.Tests;

public class AdditionalItemsTests
{
	[AdditionalItems(true)]
	private class AdditionalItemsTrue : List<int>;

	[Test]
	public void TrueIsAdded()
	{
		var buildOptions = new BuildOptions
		{
			Dialect = Dialect.Draft202012
		};
		var expected = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.AdditionalItems(true);

		var actual = new JsonSchemaBuilder(buildOptions).FromType<AdditionalItemsTrue>();

		AssertEqual(expected, actual);
	}

	[AdditionalItems(false)]
	private class AdditionalItemsFalse : List<int>;

	[Test]
	public void FalseIsAdded()
	{
		var buildOptions = new BuildOptions
		{
			Dialect = Dialect.Draft202012
		};
		var expected = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.AdditionalItems(false);

		var actual = new JsonSchemaBuilder(buildOptions).FromType<AdditionalItemsFalse>();

		AssertEqual(expected, actual);
	}

	[AdditionalItems(typeof(string))]
	private class AdditionalItemsString : List<int>;

	[Test]
	public void TypeIsAdded()
	{
		var buildOptions = new BuildOptions
		{
			Dialect = Dialect.Draft202012
		};
		var expected = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
			.AdditionalItems(new JsonSchemaBuilder().Type(SchemaValueType.String));

		var actual = new JsonSchemaBuilder(buildOptions).FromType<AdditionalItemsString>();

		AssertEqual(expected, actual);
	}

	private class IntList : List<int>
	{
		public string Bar { get; set; }
	}

	private class AdditionalItemsOnProp
	{
		[AdditionalItems(false)]
		public IntList Foo { get; set; }
	}

	[Test]
	public void FalseIsAddedToProp()
	{
		var buildOptions = new BuildOptions
		{
			Dialect = Dialect.Draft202012
		};
		var expected = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
					.AdditionalItems(false)
				)
			);

		var actual = new JsonSchemaBuilder(buildOptions).FromType<AdditionalItemsOnProp>();

		AssertEqual(expected, actual);
	}
}