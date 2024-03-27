#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class AllowedValueAttributeTests
{
	private class Target
	{
		[AllowedValues(1, 10, "string")]
		[Description("a descriptor")]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateEnum()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Enum(1, 10, "string")
					.Description("a descriptor")
				)
			);

		VerifyGeneration<Target>(expected);
	}
}

#endif