using FluentAssertions;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public class NumberRangeSetTests
	{
		[Test]
		public void DualIntersection()
		{
			var a = new NumberRangeSet(new[]
			{
				new NumberRange(-100, -10),
				new NumberRange(10, 100)
			});
			var b = new NumberRangeSet(new NumberRange(-20, 20));
			var expected = new NumberRangeSet(new[]
			{
				new NumberRange(-20, -10),
				new NumberRange(10, 20)
			});

			var result = a * b;

			result.Should().BeEquivalentTo(expected);
		}

		[Test]
		public void DualIntersectionTheOtherWay()
		{
			var a = new NumberRangeSet(new[]
			{
				new NumberRange(-100, -10),
				new NumberRange(10, 100)
			});
			var b = new NumberRangeSet(new NumberRange(-20, 20));
			var expected = new NumberRangeSet(new[]
			{
				new NumberRange(-20, -10),
				new NumberRange(10, 20)
			});

			var result = b * a;

			result.Should().BeEquivalentTo(expected);
		}
	}
}
