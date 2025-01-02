using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class StringLengthAttributeTests
{
	private class TargetNoMinimum
	{
		[StringLength(10)]
		public object Simple { get; set; }
	}

	[Test]
	public void GenerateStringRange()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.MaxLength(10)
				)
			);

		VerifyGeneration<TargetNoMinimum>(expected);
	}

	private class TargetWithMinimum
	{
		[StringLength(20, MinimumLength = 2)]
		public object WithMin { get; set; }
	}

	[Test]
	public void GenerateStringRangeMinimum()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("WithMin", new JsonSchemaBuilder()
					.MinLength(2)
					.MaxLength(20)
				)
			);

		VerifyGeneration<TargetWithMinimum>(expected);
	}
}