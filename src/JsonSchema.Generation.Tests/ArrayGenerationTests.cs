using System.Collections.Generic;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local

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

		AssertEqual(expected, actual);
	}

	private class MinItemsList
	{
		[MinItems(5)]
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

		AssertEqual(expected, actual);
	}

	private class MinValueList
	{
		[Minimum(5, GenericParameter = 0)]
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

		AssertEqual(expected, actual);
	}

	private class MinValueListWithBasicList
	{
		[Minimum(5, GenericParameter = 0)]
		public List<int> List { get; set; }
		public List<int> BasicList { get; set; }
	}

	[Test]
	public void ListOfIntWithMinValueWithBasicList()
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
				),
				("BasicList", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
					)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<MinValueListWithBasicList>();

		AssertEqual(expected, actual);
	}

	[Title("A test enum")]
	public enum EnumTest
	{
		One = 1,
		Two = 2
	}

	[Test]
	public void ListOfAttributedEnum()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Enum("One", "Two")
				.Title("A test enum")
			);

		var actual = new JsonSchemaBuilder().FromType<List<EnumTest>>();

		AssertEqual(expected, actual);
	}

	private class MultipleTestEnums
	{
		public List<EnumTest> List { get; set; }
		public EnumTest Single { get; set; }
	}

	[Test]
	public void AttributedEnumIsRefactored()
	{
		var expected = new JsonSchemaBuilder()
			.Defs(
				("enumTestInArrayGenerationTests", new JsonSchemaBuilder()
					.Enum("One", "Two")
					.Title("A test enum")
				)
			)
			.Type(SchemaValueType.Object)
			.Properties(
				("List", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Ref("#/$defs/enumTestInArrayGenerationTests"))
				),
				("Single", new JsonSchemaBuilder().Ref("#/$defs/enumTestInArrayGenerationTests"))
			);

		var actual = new JsonSchemaBuilder().FromType<MultipleTestEnums>();

		AssertEqual(expected, actual);
	}
}