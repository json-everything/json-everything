using System;
using NUnit.Framework;

namespace Json.Path.Tests;

public class JsonPointerTests
{
	[Test]
	public void Name()
	{
		var path = JsonPath.Parse("$['foo']");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/foo", asPointer);
	}

	[Test]
	public void Name_Shorthand()
	{
		var path = JsonPath.Parse("$.foo");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/foo", asPointer);
	}

	[Test]
	public void Index()
	{
		var path = JsonPath.Parse("$[1]");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/1", asPointer);
	}

	[Test]
	public void MultipleSegments()
	{
		var path = JsonPath.Parse("$[1].foo");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/1/foo", asPointer);
	}

	[Test]
	public void NameWithTilde()
	{
		var path = JsonPath.Parse("$['~foo']");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/~0foo", asPointer);
	}

	[Test]
	public void NameWithSlash()
	{
		var path = JsonPath.Parse("$['/foo']");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/~1foo", asPointer);
	}

	[Test]
	public void NameWithSurrogatePair()
	{
		var path = JsonPath.Parse("$['\\uD834\\uDD1E']");

		var asPointer = path.AsJsonPointer();

		Assert.AreEqual("/𝄞", asPointer);
	}

	[Test]
	public void Slice()
	{
		var path = JsonPath.Parse("$[1:2]");

		Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
	}

	[Test]
	public void RecursiveDescent()
	{
		var path = JsonPath.Parse("$..foo");

		Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
	}

	[Test]
	public void Wildcard()
	{
		var path = JsonPath.Parse("$[*]");

		Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
	}

	[Test]
	public void Wildcard_Shorthand()
	{
		var path = JsonPath.Parse("$.*");

		Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
	}

	[Test]
	public void MultipleSelectors()
	{
		var path = JsonPath.Parse("$[1,'foo']");

		Assert.Throws<InvalidOperationException>(() => path.AsJsonPointer());
	}
}