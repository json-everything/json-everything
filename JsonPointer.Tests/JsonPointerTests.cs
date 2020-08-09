using System;
using System.Collections;
using System.Text.Json;
using NUnit.Framework;

namespace JsonPointer.Tests
{
	[TestFixture]
	public class JsonPointerTests
	{
		public static IEnumerable ErrorCases
		{
			get
			{
				yield return new TestCaseData("/d", "/d");
				yield return new TestCaseData("/a/0", "/a/0");
				yield return new TestCaseData("/d/something", "/d");
				yield return new TestCaseData("/b/false", "/b/false");
				yield return new TestCaseData("/b/-1", "/b/-1");
				yield return new TestCaseData("/b/5", "/b/5");
				yield return new TestCaseData("/c/1", "/c/1");
				yield return new TestCaseData("/b/001", "/b/001");
			}
		}

		[TestCaseSource(nameof(ErrorCases))]
		public void Errors(string pointerString, string expectedError)
		{
			var target = JsonDocument.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"false\":false}}");

			var pointer = Json.Pointer.JsonPointer.Parse(pointerString);

			var actual = pointer.Evaluate(target.RootElement);

			Assert.IsNull(actual);
		}

		[Test]
		public void IndexingAnObjectInterpretsIndexAsKey()
		{
			var target = JsonDocument.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"0\":false}}");

			var pointer = Json.Pointer.JsonPointer.Parse("/c/0");

			var actual = pointer.Evaluate(target.RootElement);

			Assert.AreEqual(false, actual.Value.GetBoolean());
		}

		[Test]
		public void GettingLastItemInArray()
		{
			var target = JsonDocument.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"0\":false}}");

			var pointer = Json.Pointer.JsonPointer.Parse("/b/-");

			var actual = pointer.Evaluate(target.RootElement);

			Assert.AreEqual(JsonValueKind.Null, actual.Value.ValueKind);
		}
	}
}