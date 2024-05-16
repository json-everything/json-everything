using NUnit.Framework;

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
		var schema = new JsonSchemaBuilder()
			.FromType<BoundaryTestSubject>()
			.Build();

		Assert.Multiple(() =>
		{
			Assert.That(schema.GetProperties()!["Value"].GetExclusiveMaximum(), Is.EqualTo(decimal.MaxValue));
			Assert.That(schema.GetProperties()!["Value"].GetExclusiveMinimum(), Is.EqualTo(decimal.MinValue));
			Assert.That(schema.GetProperties()!["Value"].GetMaximum(), Is.EqualTo(decimal.MaxValue));
			Assert.That(schema.GetProperties()!["Value"].GetMinimum(), Is.EqualTo(decimal.MinValue));
		});
	}
}