using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NUnit.Framework;
// ReSharper disable CollectionNeverUpdated.Local

namespace Json.Pointer.Tests;

public class ExpressionCreationTests
{
	private class TestClass
	{
		public string String { get; set; }
		[JsonPropertyName("customName")]
		public string Other { get; set; }
		public List<int> Ints { get; set; }
		public TestClass Nest { get; set; }
		public List<TestClass> NestMore { get; set; }
		public string[] StringArray { get; set; }
	}

	[Test]
	public void SimpleProperty()
	{
		var expected = "/String";
		var actual = JsonPointer.Create<TestClass>(x => x.String);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void JsonProperty()
	{
		var expected = "/customName";
		var actual = JsonPointer.Create<TestClass>(x => x.Other);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void SimpleArrayIndex()
	{
		var expected = "/Ints/1";
		var actual = JsonPointer.Create<TestClass>(x => x.Ints[1]);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void SimpleArrayIndexWithAnActualArray()
	{
		var expected = "/StringArray/2";
		var actual = JsonPointer.Create<TestClass>(x => x.StringArray[2]);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void NestedProperty()
	{
		var expected = "/Nest/Nest";
		var actual = JsonPointer.Create<TestClass>(x => x.Nest.Nest);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void NestedWithArrayIndexProperty()
	{
		var expected = "/Nest/Ints/5";
		var actual = JsonPointer.Create<TestClass>(x => x.Nest.Ints[5]);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void ArrayIndexWithNestProperty()
	{
		var expected = "/NestMore/5/Nest";
		var actual = JsonPointer.Create<TestClass>(x => x.NestMore[5].Nest);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void LastArrayIndex()
	{
		var expected = "/NestMore/-";
		var actual = JsonPointer.Create<TestClass>(x => x.NestMore.Last());

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void LastArrayIndexWithNestProperty()
	{
		var expected = "/NestMore/-/Nest";
		var actual = JsonPointer.Create<TestClass>(x => x.NestMore.Last().Nest);

		Assert.AreEqual(expected, actual.ToString());
	}
}