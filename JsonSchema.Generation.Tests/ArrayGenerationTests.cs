using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests;

public class ArrayGenerationTests
{
	[Test]
	public void ListOfInt()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
			);

		var actual = new JsonSchemaBuilder().FromType<List<int>>();

		AssertionExtensions.AssertEqual(expected, actual);
	}

	[UsedImplicitly]
	private class MinItemsList
	{
		[MinItems(5)]
		[UsedImplicitly]
		public List<int> List { get; set; }
	}

	[Test]
	public void ListOfIntWithMinItems()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("List", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
					)
					.MinItems(5)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<MinItemsList>();

		AssertionExtensions.AssertEqual(expected, actual);
	}

	[UsedImplicitly]
	private class MinValueList
	{
		[Minimum(5)]
		[UsedImplicitly]
		public List<int> List { get; set; }
	}

	[Test]
	public void ListOfIntWithMinValue()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("List", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Minimum(5)
					)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<MinValueList>();

		AssertionExtensions.AssertEqual(expected, actual);
	}
}