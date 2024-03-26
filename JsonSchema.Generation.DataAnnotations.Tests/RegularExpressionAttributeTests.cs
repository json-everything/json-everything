using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Tests;
using NUnit.Framework;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class RegularExpressionAttributeTests
{
	private class Target
	{
		[RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$")]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateMinLength()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Pattern(@"^[a-zA-Z''-'\s]{1,40}$")
				)
			);

		AssertionExtensions.VerifyGeneration<Target>(expected);
	}
}