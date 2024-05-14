using System;
using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Tests;
using NUnit.Framework;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class EnumDataTypeAttributeTests
{
	private class Target
	{
		[EnumDataType(typeof(DayOfWeek))]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateEnum()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Enum(Enum.GetNames(typeof(DayOfWeek)))
				)
			);

		AssertionExtensions.VerifyGeneration<Target>(expected);
	}
}