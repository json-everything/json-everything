using FluentAssertions;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests;

public class NumberRangeSetTests
{
	[Test]
	public void DualIntersection()
	{
		var a = new NumberRangeSet(
		[
			new NumberRange(-100, -10),
			new NumberRange(10, 100)
		]);
		var b = new NumberRangeSet(new NumberRange(-20, 20));
		var expected = new NumberRangeSet(
		[
			new NumberRange(-20, -10),
			new NumberRange(10, 20)
		]);

		var result = a * b;

		result.Should().BeEquivalentTo(expected);
	}

	[Test]
	public void DualIntersectionTheOtherWay()
	{
		var a = new NumberRangeSet(
		[
			new NumberRange(-100, -10),
			new NumberRange(10, 100)
		]);
		var b = new NumberRangeSet(new NumberRange(-20, 20));
		var expected = new NumberRangeSet(
		[
			new NumberRange(-20, -10),
			new NumberRange(10, 20)
		]);

		var result = b * a;

		result.Should().BeEquivalentTo(expected);
	}

	[Test]
	public void UnionOfDisjointRanges()
	{
		var a = new NumberRangeSet(new NumberRange(1, 5));
		var b = new NumberRangeSet(new NumberRange(10, 20));

		var result = a + b;

		Assert.That(result.Ranges, Has.Count.EqualTo(2));
		Assert.That(result.Ranges[0], Is.EqualTo(new NumberRange(1, 5)));
		Assert.That(result.Ranges[1], Is.EqualTo(new NumberRange(10, 20)));
	}

	[Test]
	public void SubtractProducesCorrectDifference()
	{
		var a = new NumberRangeSet(new NumberRange(0, 100));
		var b = new NumberRangeSet(new NumberRange(40, 60));

		var result = a - b;

		Assert.That(result.Ranges, Has.Count.EqualTo(2));
		Assert.That(result.Ranges[0].Minimum.Value, Is.EqualTo(0));
		Assert.That(result.Ranges[1].Maximum.Value, Is.EqualTo(100));
	}

	[Test]
	public void IntersectionOfDisjointRangesIsEmpty()
	{
		var a = new NumberRangeSet(new NumberRange(1, 5));
		var b = new NumberRangeSet(new NumberRange(10, 20));

		var result = a * b;

		Assert.That(result.Ranges, Is.Empty);
	}

	[Test]
	public void GetComplementInvertsRange()
	{
		var a = new NumberRangeSet(new NumberRange(0, 1000000m));

		var complement = a.GetComplement();

		Assert.That(complement.Ranges, Has.Count.GreaterThanOrEqualTo(1));
		Assert.That(complement.Ranges[0].Maximum.Value, Is.LessThanOrEqualTo(0));
	}

	[Test]
	public void FloorCapsMinimum()
	{
		var a = NumberRangeSet.Full;

		var result = a.Floor(5);

		Assert.That(result.Ranges, Has.Count.EqualTo(1));
		Assert.That(result.Ranges[0].Minimum.Value, Is.EqualTo(5));
	}

	[Test]
	public void CeilingCapsMaximum()
	{
		var a = NumberRangeSet.Full;

		var result = a.Ceiling(50);

		Assert.That(result.Ranges, Has.Count.EqualTo(1));
		Assert.That(result.Ranges[0].Maximum.Value, Is.EqualTo(50));
	}
}