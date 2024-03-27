using System;
using NUnit.Framework;

using DARange = System.ComponentModel.DataAnnotations.RangeAttribute;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class RangeAttributeTests
{
	private class SupportedTarget
	{
		[DARange(1, 10)]
		public object Simple { get; set; }
	}

	[Test]
	public void SupportedRangeUsages()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Minimum(1)
					.Maximum(10)
				)
			);

		VerifyGeneration<SupportedTarget>(expected);
	}

	private class UnsupportedTarget
	{
		[DARange(typeof(DateTime), "2020/3/1", "2020/6/1")]
		public object WithType { get; set; }
	}

	[Test]
	public void UnsupportedRangeUsages()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("WithType", true)
			);

		VerifyGeneration<UnsupportedTarget>(expected);
	}
}