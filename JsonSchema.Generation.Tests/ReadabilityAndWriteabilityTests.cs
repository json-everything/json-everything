using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class ReadabilityAndWritabilityTests
{
	private class Target
	{
		private int _writeOnlyProperty;

		public int ReadOnlyProperty { get; }

		public int WriteOnlyProperty
		{
			set => _writeOnlyProperty = value;
		}
	}

	[Test]
	public void TargetGetsAllCharacteristicsRight()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("ReadOnlyProperty", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.ReadOnly(true)
				),
				("WriteOnlyProperty", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.WriteOnly(true)
				)
			);

		var actual = new JsonSchemaBuilder().FromType<Target>();

		AssertEqual(expected, actual);
	}
}