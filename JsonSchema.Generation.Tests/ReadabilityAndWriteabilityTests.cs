using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class ReadabilityAndWritabilityTests
{
	private class ReadOnlyProperty
	{
		public int Prop { get; }
	}

	[Test]
	public void ReadOnlyPropertyTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.ReadOnly(true)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<ReadOnlyProperty>();

		AssertEqual(expected, actual);
	}

	private class ReadOnlyPropertyWithReadOnlyAttributeFalse
	{
		[ReadOnly(false)] public int Prop { get; }
	}

	[Test]
	public void ReadOnlyPropertyWithReadOnlyAttributeFalseTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.ReadOnly(false)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<ReadOnlyPropertyWithReadOnlyAttributeFalse>();

		AssertEqual(expected, actual);
	}

	private class ReadWritePropertyWithReadOnlyAttribute
	{
		[ReadOnly(true)] public int Prop { get; set; }
	}

	[Test]
	public void ReadWritePropertyWithReadOnlyAttributeTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.ReadOnly(true)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<ReadWritePropertyWithReadOnlyAttribute>();

		AssertEqual(expected, actual);
	}

	private class ReadWritePropertyWithReadOnlyAttributeFalse
	{
		[ReadOnly(false)] public int Prop { get; set; }

	}

	[Test]
	public void ReadWritePropertyWithReadOnlyAttributeFalseTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.ReadOnly(false)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<ReadWritePropertyWithReadOnlyAttributeFalse>();

		AssertEqual(expected, actual);
	}

	private class WriteOnlyProperty
	{
		public int Prop
		{
			set { }
		}
	}

	[Test]
	public void WriteOnlyPropertyTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.WriteOnly(true)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<WriteOnlyProperty>();

		AssertEqual(expected, actual);
	}

	private class WriteOnlyPropertyWithWriteOnlyAttributeFalse
	{
		[WriteOnly(false)]
		public int Prop
		{
			set { }
		}
	}

	[Test]
	public void WriteOnlyPropertyWithWriteOnlyAttributeFalseTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.WriteOnly(false)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<WriteOnlyPropertyWithWriteOnlyAttributeFalse>();

		AssertEqual(expected, actual);
	}

	private class ReadWritePropertyWithWriteOnlyAttribute
	{
		[WriteOnly(true)] public int Prop { get; set; }
	}

	[Test]
	public void ReadWritePropertyWithWriteOnlyAttributeTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.WriteOnly(true)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<ReadWritePropertyWithWriteOnlyAttribute>();

		AssertEqual(expected, actual);
	}

	private class ReadWritePropertyWithWriteOnlyAttributeFalse
	{
		[WriteOnly(false)] public int Prop { get; set; }
	}

	[Test]
	public void ReadWritePropertyWithWriteOnlyAttributeFalseTest()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Prop", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.WriteOnly(false)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<ReadWritePropertyWithWriteOnlyAttributeFalse>();

		AssertEqual(expected, actual);
	}
}