using NUnit.Framework;

namespace Json.Pointer.Tests;

public class ManipulationTests
{
	[TestCase("/foo/5/bar", 0, "/foo/5/bar")]
	[TestCase("/foo/5/bar", 1, "/foo/5")]
	[TestCase("/foo/5/bar", 2, "/foo")]
	[TestCase("/foo/5/bar", 3, "")]
	public void Ancestor(string original, int levels, string expected)
	{
		var pointer = JsonPointer.Parse(original);

		var actual = pointer.GetParent(levels);

		Assert.That(actual.ToString(), Is.EqualTo(expected));
	}

	[TestCase("/foo/5/bar", 0, "/foo/5/bar")]
	[TestCase("/foo/5/bar", 1, "/5/bar")]
	[TestCase("/foo/5/bar", 2, "/bar")]
	[TestCase("/foo/5/bar", 3, "")]
	public void Local(string original, int skip, string expected)
	{
		var pointer = JsonPointer.Parse(original);

		var actual = pointer.GetLocal(skip);

		Assert.That(actual.ToString(), Is.EqualTo(expected));
	}

	[TestCase("/foo/5/bar", "/baz/42/quux", "/foo/5/bar/baz/42/quux")]
	public void CombinePointer(string a, string b, string expected)
	{
		var left = JsonPointer.Parse(a);
		var right = JsonPointer.Parse(b);

		var actual = left.Combine(right);

		Assert.That(actual.ToString(), Is.EqualTo(expected));
	}

	[Test]
	public void CombineSegments()
	{
		var pointer = JsonPointer.Parse("/foo/5/bar");

		var actual = pointer.Combine("baz", 0, "quux", 5, 10);

		Assert.That(actual.ToString(), Is.EqualTo("/foo/5/bar/baz/0/quux/5/10"));
	}
}