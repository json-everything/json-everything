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

		Assert.AreEqual(decimal.MaxValue, schema.GetProperties()!["Value"].GetExclusiveMaximum());
		Assert.AreEqual(decimal.MinValue, schema.GetProperties()!["Value"].GetExclusiveMinimum());
		Assert.AreEqual(decimal.MaxValue, schema.GetProperties()!["Value"].GetMaximum());
		Assert.AreEqual(decimal.MinValue, schema.GetProperties()!["Value"].GetMinimum());
	}
}