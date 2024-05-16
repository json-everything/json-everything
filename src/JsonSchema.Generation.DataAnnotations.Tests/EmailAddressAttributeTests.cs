using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class EmailAddressAttributeTests
{
	private class Target
	{
		[EmailAddress]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateEmailFormat()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Format(Formats.Email)
				)
			);

		VerifyGeneration<Target>(expected);
	}
}