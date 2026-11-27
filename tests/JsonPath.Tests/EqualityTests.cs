using NUnit.Framework;

namespace Json.Path.Tests;

public class EqualityTests
{
	[TestCase("$", "$")]
	[TestCase("$.foo", "$.foo")]
	[TestCase("$.foo", "$['foo']")]
	[TestCase("$.foo.bar", "$['foo']['bar']")]
	[TestCase("$.foo[0]", "$['foo'][0]")]
	[TestCase("$.foo.*", "$['foo'][*]")]
	[TestCase("$..foo", "$..['foo']")]
	[TestCase("$[1:5:2]", "$[1:5:2]")]
	[TestCase("$[0,1,'a']", "$[0,1,'a']")]
	[TestCase("$[?@.a == 1]", "$[?@['a'] == 1]")]
	[TestCase("$[?@.a == 1 && @.b != 'x']", "$[?@['a']==1&&@['b']!='x']")]
	[TestCase("$[?!@.a]", "$[?!@['a']]")]
	[TestCase("$[?length(@.a) > 2]", "$[?length(@['a']) > 2]")]
	[TestCase("$[?match(@.a, 'foo')]", "$[?match(@['a'], 'foo')]")]
	public void EqualPaths_AreEqualAndHaveSameHashCode(string leftText, string rightText)
	{
		var left = JsonPath.Parse(leftText);
		var right = JsonPath.Parse(rightText);

		Assert.That(left.Equals(right), Is.True);
		Assert.That(right.Equals(left), Is.True);
		Assert.That(left == right, Is.True);
		Assert.That(left != right, Is.False);
		Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
	}

	[TestCase("$", "$.foo")]
	[TestCase("$.foo", "$.bar")]
	[TestCase("$.foo", "$..foo")]
	[TestCase("$[0]", "$[1]")]
	[TestCase("$[0]", "$['0']")]
	[TestCase("$[1:5]", "$[1:5:2]")]
	[TestCase("$[0,1]", "$[1,0]")]
	[TestCase("$[?@.a == 1]", "$[?@.a == 2]")]
	[TestCase("$[?@.a == 1]", "$[?@.a != 1]")]
	[TestCase("$[?@.a == 1]", "$[?@.b == 1]")]
	[TestCase("$[?@.a && @.b]", "$[?@.a || @.b]")]
	[TestCase("$[?@.a]", "$[?!@.a]")]
	[TestCase("$[?length(@.a) > 2]", "$[?count(@.a) > 2]")]
	public void DifferentPaths_AreNotEqual(string leftText, string rightText)
	{
		var left = JsonPath.Parse(leftText);
		var right = JsonPath.Parse(rightText);

		Assert.That(left.Equals(right), Is.False);
		Assert.That(right.Equals(left), Is.False);
		Assert.That(left == right, Is.False);
		Assert.That(left != right, Is.True);
	}

	[Test]
	public void AppendedPath_EqualsParsedShorthandPath()
	{
		var appended = JsonPath.Root.Append("foo").Append(0).Append("bar");
		var parsed = JsonPath.Parse("$.foo[0].bar");

		Assert.That(appended, Is.EqualTo(parsed));
		Assert.That(appended.GetHashCode(), Is.EqualTo(parsed.GetHashCode()));
	}

	[Test]
	public void NullComparison_HandledCorrectly()
	{
		var path = JsonPath.Parse("$.foo");
		JsonPath? nullPath = null;

		Assert.That(path.Equals(nullPath), Is.False);
		Assert.That(path == nullPath, Is.False);
		Assert.That(nullPath == path, Is.False);
		Assert.That(path != nullPath, Is.True);
		Assert.That(nullPath == null, Is.True);
	}

	[Test]
	public void PathSegment_ShorthandAndBracket_AreEqual()
	{
		var shorthand = JsonPath.Parse("$.foo").Segments[0];
		var bracket = JsonPath.Parse("$['foo']").Segments[0];
		var recursive = JsonPath.Parse("$..foo").Segments[0];

		Assert.That(shorthand.Equals(bracket), Is.True);
		Assert.That(shorthand == bracket, Is.True);
		Assert.That(shorthand.GetHashCode(), Is.EqualTo(bracket.GetHashCode()));
		Assert.That(shorthand.Equals(recursive), Is.False);
		Assert.That(shorthand != recursive, Is.True);
	}
}
