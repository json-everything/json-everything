using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;

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

		VerifyGeneration<Target>(expected);
	}
}