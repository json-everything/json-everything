using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Pointer.Tests
{
	public class ExpressionCreationTests
	{
		class TestClass
		{
			public string String { get; set; }
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
	}
}
