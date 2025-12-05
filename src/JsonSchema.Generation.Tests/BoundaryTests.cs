using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Generation.Tests;

public class BoundaryTests
{
	// ReSharper disable once ClassNeverInstantiated.Local
	private class BoundaryTestSubject
	{
		[ExclusiveMaximum(double.MaxValue)]
		[ExclusiveMinimum(double.MinValue)]
		[Maximum(double.MaxValue)]
		[Minimum(double.MinValue)]
		// ReSharper disable once UnusedMember.Local
		public int Value { get; set; }
	}

	[Test]
	public void MaximumRangeIsClamped()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<BoundaryTestSubject>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var maxValue = decimal.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
		var minValue = decimal.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

		var expected = JsonDocument.Parse(
			$$"""
			{
			  "type": "object",
			  "properties": {
			    "Value": {
			      "type": "integer",
			      "minimum": {{minValue}},
			      "maximum": {{maxValue}},
			      "exclusiveMinimum": {{minValue}},
			      "exclusiveMaximum": {{maxValue}}
			    }
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}
}