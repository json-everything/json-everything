#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Tests;
using NUnit.Framework;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class Base64StringAttributeTests
{
	private class Target
	{
		[Base64String]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateEmailFormat()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Format("base64")
				)
			);

		AssertionExtensions.VerifyGeneration<Target>(expected);
	}
}

#endif