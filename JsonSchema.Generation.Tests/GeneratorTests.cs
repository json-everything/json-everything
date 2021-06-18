using System;
using System.Collections.Generic;
using System.Linq;
using Json.More;
using Json.Pointer;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests
{
	public class GeneratorTests
	{
		[Test]
		public void ArraySchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				.Build();

			var actual = new JsonSchemaBuilder().FromType<List<string>>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void BooleanSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Boolean)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<bool>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void DateTimeSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.DateTime)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<DateTime>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void EnumDictionarySchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.PropertyNames(new JsonSchemaBuilder().Enum(Enum.GetNames(typeof(DayOfWeek)).Select(x => x.AsJsonElement())))
				.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				.Build();

			var actual = new JsonSchemaBuilder().FromType<Dictionary<DayOfWeek, int>>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void EnumSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Enum(Enum.GetNames(typeof(DayOfWeek)).Select(x => x.AsJsonElement()))
				.Build();

			var actual = new JsonSchemaBuilder().FromType<DayOfWeek>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void GuidSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Uuid)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<Guid>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void IntegerSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<int>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void JsonPointerSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.JsonPointer)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<JsonPointer>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NumberSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<double>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void StringSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<string>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void UriSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Uri)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<Uri>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NullableBooleanSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Boolean)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<bool?>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NullableDateTimeSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.DateTime)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<DateTime?>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NullableEnumSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Enum(Enum.GetNames(typeof(DayOfWeek)).Select(x => x.AsJsonElement()))
				.Build();

			var actual = new JsonSchemaBuilder().FromType<DayOfWeek?>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NullableGuidSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Format(Formats.Uuid)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<Guid?>().Build();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NullableIntegerSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<int?>().Build();

			AssertEqual(expected, actual);
		}


		[Test]
		public void NullableNumberSchema()
		{
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<double?>().Build();

			AssertEqual(expected, actual);
		}
	}
}
