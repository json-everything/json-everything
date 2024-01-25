using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NUnit.Framework;
// ReSharper disable CollectionNeverUpdated.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

	public static IEnumerable<TestCaseData> NamingOptions
	{
		get
		{
			yield return new TestCaseData(PropertyNameResolvers.AsDeclared, "/NestMore");
			yield return new TestCaseData(PropertyNameResolvers.CamelCase, "/nestMore");
			yield return new TestCaseData(PropertyNameResolvers.KebabCase, "/nest-more");
			yield return new TestCaseData(PropertyNameResolvers.PascalCase, "/NestMore");
			yield return new TestCaseData(PropertyNameResolvers.SnakeCase, "/nest_more");
			yield return new TestCaseData(PropertyNameResolvers.UpperKebabCase, "/NEST-MORE");
			yield return new TestCaseData(PropertyNameResolvers.UpperSnakeCase, "/NEST_MORE");
		}
	}

	[TestCaseSource(nameof(NamingOptions))]
	public void SimplePropertyWithOptions(PropertyNameResolver resolver, string expected)
	{
		var actual = JsonPointer.Create<TestClass>(x => x.NestMore, new PointerCreationOptions { PropertyNameResolver = resolver });

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

	[Test]
	public void ArrayIndexUsingVariable()
	{
		var index = 5;

		var expected = "/NestMore/5/Nest";
		var actual = JsonPointer.Create<TestClass>(x => x.NestMore[index].Nest);

		Assert.AreEqual(expected, actual.ToString());
	}

	[Test]
	public void ArrayIndexUsingMember()
	{
		var index = new { Foo = 5 };

		var expected = "/NestMore/5/Nest";
		var actual = JsonPointer.Create<TestClass>(x => x.NestMore[index.Foo].Nest);

		Assert.AreEqual(expected, actual.ToString());
	}
}