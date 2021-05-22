using System;
using System.Collections.Generic;
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
	}
}
