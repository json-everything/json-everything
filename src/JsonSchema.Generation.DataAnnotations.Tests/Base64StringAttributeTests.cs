#if NET8_0_OR_GREATER

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;

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

		VerifyGeneration<Target>(expected);
	}
}

#endif