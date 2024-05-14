using System.ComponentModel.DataAnnotations;
using Json.Schema.Generation.Tests;
using NUnit.Framework;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class UrlAttributeTests
{
	private class Target
	{
		[Url]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateEmailFormat()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Format(Formats.Uri)
				)
			);

		AssertionExtensions.VerifyGeneration<Target>(expected);
	}
}