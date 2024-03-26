using Json.Schema.Generation.Tests;
using NUnit.Framework;

using DAMaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class MaxLengthAttributeTests
{
	private class Target
	{
		[DAMaxLength(5)]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateMaxLength()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.MaxLength(5)
				)
			);

		AssertionExtensions.VerifyGeneration<Target>(expected);
	}
}