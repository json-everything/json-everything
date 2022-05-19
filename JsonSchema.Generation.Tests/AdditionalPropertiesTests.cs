using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class AdditionalPropertiesTests
{
	[AdditionalProperties(true)]
	private class AdditionalPropertiesTrue
	{
	}

	[Test]
	public void TrueIsAdded()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.AdditionalProperties(true);

		var actual = new JsonSchemaBuilder().FromType<AdditionalPropertiesTrue>();

		AssertEqual(expected, actual);
	}

	[AdditionalProperties(false)]
	private class AdditionalPropertiesFalse
	{
		public int Foo { get; set; }
	}

	[Test]
	public void FalseIsAdded()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)))
			.AdditionalProperties(false);

		var actual = new JsonSchemaBuilder().FromType<AdditionalPropertiesFalse>();

		AssertEqual(expected, actual);
	}

	[AdditionalProperties(typeof(string))]
	private class AdditionalPropertiesString
	{
		public int Foo { get; set; }
	}

	[Test]
	public void TypeIsAdded()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)))
			.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.String));

		var actual = new JsonSchemaBuilder().FromType<AdditionalPropertiesString>();

		AssertEqual(expected, actual);
	}

	private class PropClass
	{
		public string Bar { get; set; }
	}

	private class AdditionalPropertiesOnProp
	{
		[AdditionalProperties(false)]
		public PropClass Foo { get; set; }
	}

	[Test]
	public void FalseIsAddedToProp()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Properties(
						("Bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
					.AdditionalProperties(false)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<AdditionalPropertiesOnProp>();

		AssertEqual(expected, actual);
	}
}