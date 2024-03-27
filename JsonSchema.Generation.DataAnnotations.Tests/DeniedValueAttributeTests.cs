#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class DeniedValueAttributeTests
{
	private class Target
	{
		[DeniedValues(1, 10, "string")]
		[Description("a descriptor")]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateNotEnum()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Not(new JsonSchemaBuilder()
						.Enum(1, 10, "string")
					)
					.Description("a descriptor")
				)
			);

		VerifyGeneration<Target>(expected);
	}
}

#endif