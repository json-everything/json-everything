using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Tests;
using NUnit.Framework;

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

		AssertionExtensions.VerifyGeneration<Target>(expected);
	}
}