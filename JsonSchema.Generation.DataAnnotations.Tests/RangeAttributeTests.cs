using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using DARange = System.ComponentModel.DataAnnotations.RangeAttribute;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.DataAnnotations.Tests;

public class RangeAttributeTests
{
	private class SupportedRangedProperties
	{
		[DARange(1, 10)]
		public int Simple { get; set; }
		[DARange(2, 20)]
		public double WithError { get; set; }
	}

	[Test]
	public void SupportedRangeUsages()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Simple", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(1)
					.Maximum(10)
				),
				("WithError", new JsonSchemaBuilder()
					.Type(SchemaValueType.Number)
					.Minimum(2)
					.Maximum(20)
				)
			);

		JsonSchema schema = new JsonSchemaBuilder().FromType<SupportedRangedProperties>();

		AssertEqual(expected, schema);
	}

	private class UnsupportedRangedProperties
	{
		[DARange(typeof(DateTime), "2020/3/1", "2020/6/1")]
		public DateTime WithType { get; set; }
	}
}