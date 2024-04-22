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

		var actual = pointer.GetAncestor(levels);

		Assert.AreEqual(expected, actual.ToString());
	}

	[TestCase("/foo/5/bar", 0, "/foo/5/bar")]
	[TestCase("/foo/5/bar", 1, "/5/bar")]
	[TestCase("/foo/5/bar", 2, "/bar")]
	[TestCase("/foo/5/bar", 3, "")]
	public void Local(string original, int skip, string expected)
	{
		var pointer = JsonPointer.Parse(original);

		var actual = pointer.GetLocal(skip);

		Assert.AreEqual(expected, actual.ToString());
	}

	[TestCase("/foo/5/bar", "/baz/42/quux", "/foo/5/bar/baz/42/quux")]
	public void CombinePointer(string a, string b, string expected)
	{
		var left = JsonPointer.Parse(a);
		var right = JsonPointer.Parse(b);

		var actual = left.Combine(right);

		Assert.AreEqual(expected, actual.ToString());
	}
}