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

		Assert.That(asPointer, Is.EqualTo("/foo"));
	}

	[Test]
	public void Name_Shorthand()
	{
		var path = JsonPath.Parse("$.foo");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/foo"));
	}

	[Test]
	public void Index()
	{
		var path = JsonPath.Parse("$[1]");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/1"));
	}

	[Test]
	public void MultipleSegments()
	{
		var path = JsonPath.Parse("$[1].foo");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/1/foo"));
	}

	[Test]
	public void NameWithTilde()
	{
		var path = JsonPath.Parse("$['~foo']");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/~0foo"));
	}

	[Test]
	public void NameWithSlash()
	{
		var path = JsonPath.Parse("$['/foo']");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/~1foo"));
	}

	[Test]
	public void NameWithSurrogatePair()
	{
		var path = JsonPath.Parse("$['\\uD834\\uDD1E']");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/𝄞"));
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

	[Test]
	public void MultipleSelectorsButSameValue_IndexFirst()
	{
		var path = JsonPath.Parse("$[1,'1']");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/1"));
	}

	[Test]
	public void MultipleSelectorsButSameValue_NameFirst()
	{
		var path = JsonPath.Parse("$['1',1]");

		var asPointer = path.AsJsonPointer();

		Assert.That(asPointer, Is.EqualTo("/1"));
	}
}