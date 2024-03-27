#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class LengthAttributeTests
{
	private class Target
	{
		[Length(1, 10)]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateStringRange()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.MinLength(1)
					.MaxLength(10)
				)
			);

		VerifyGeneration<Target>(expected);
	}
}

#endif