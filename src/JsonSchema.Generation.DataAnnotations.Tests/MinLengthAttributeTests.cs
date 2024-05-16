using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;
using DAMinLength = System.ComponentModel.DataAnnotations.MinLengthAttribute;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class MinLengthAttributeTests
{
	private class Target
	{
		[DAMinLength(5)]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateMinLength()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.MinLength(5)
				)
			);

		VerifyGeneration<Target>(expected);
	}
}