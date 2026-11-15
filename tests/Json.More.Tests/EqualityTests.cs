using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.More.Tests;

public class EqualityTests
{
	[Test]
	public void NumbersAreEqualEvenIfOneHasTrailingZero()
	{
		JsonNode intFour = 4;
		JsonNode floatFour = 4.0;

		Assert.That(intFour.IsEquivalentTo(floatFour), Is.True);
	}

	[Test]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_Decimal()
	{
		JsonNode intFour = (decimal) 4;
		JsonNode floatFour = (decimal) 4.0;

		Assert.That(intFour.IsEquivalentTo(floatFour), Is.True);
	}

	[Test]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_IntAndDecimal()
	{
		JsonNode intFour = 4;
		JsonNode floatFour = 4.0m;

		Assert.That(intFour.IsEquivalentTo(floatFour), Is.True);
	}

	[Test]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_Summed()
	{
		decimal a = 4;
		decimal b = (decimal)4.00;
		JsonNode explicitFour = a;
		JsonNode summedFour = b;

		Assert.That(explicitFour.IsEquivalentTo(summedFour), Is.True);
	}

	[Test]
	public void NumbersAreEqualEvenIfOneHasTrailingZero_Parsed()
	{
		var intFour = JsonNode.Parse("4");
		var floatFour = JsonNode.Parse("4.0");

		Assert.That(intFour.IsEquivalentTo(floatFour), Is.True);
	}
}
