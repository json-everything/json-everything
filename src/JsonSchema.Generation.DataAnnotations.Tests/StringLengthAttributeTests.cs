using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class StringLengthAttributeTests
{
	private class Target
	{
		[StringLength(10)]
		public object Simple { get; set; }
		[StringLength(20, MinimumLength = 2)]
		public object WithMin { get; set; }
	}

	[Test]
	public void GenerateStringRange()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.MaxLength(10)
				),
				("WithMin", new JsonSchemaBuilder()
					.MinLength(2)
					.MaxLength(20)
				)
			);

		VerifyGeneration<Target>(expected);
	}
}