using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

internal class TypeAttributeTests
{
	// ReSharper disable once ClassNeverInstantiated.Local
	// ReSharper disable UnusedMember.Local
	[Title("an object with attributes")]
	[Description("this object has attributes")]
	[ReadOnly]
	private class AttributedObject
	{
		[Title("a property with attributes")]
		public int Value { get; set; }
	}
	// ReSharper restore UnusedMember.Local

	[Test]
	public void AttributedObjectHasTitleAndDescription()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Title("an object with attributes")
			.Description("this object has attributes")
			.Properties(
				("Value", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Title("a property with attributes")
				)
			)
			.ReadOnly(true);

		var actual = new JsonSchemaBuilder().FromType<AttributedObject>();

		AssertEqual(expected, actual);
	}
}