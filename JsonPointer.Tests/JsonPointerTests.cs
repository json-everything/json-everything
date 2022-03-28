using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Pointer.Tests;

[TestFixture]
public class JsonPointerTests
{
	public static IEnumerable ErrorCases
	{
		get
		{
			yield return new TestCaseData("/d");
			yield return new TestCaseData("/a/0");
			yield return new TestCaseData("/d/something");
			yield return new TestCaseData("/b/false");
			yield return new TestCaseData("/b/-1");
			yield return new TestCaseData("/b/5");
			yield return new TestCaseData("/c/1");
			yield return new TestCaseData("/b/001");
		}
	}

	[TestCaseSource(nameof(ErrorCases))]
	public void Errors(string pointerString)
	{
		using var target = JsonDocument.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"false\":false}}");

		var pointer = JsonPointer.Parse(pointerString);

		var actual = pointer.Evaluate(target.RootElement);

		Assert.IsNull(actual);
	}

	[TestCaseSource(nameof(ErrorCases))]
	public void Errors_ButWithNodes(string pointerString)
	{
		var target = JsonNode.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"false\":false}}")!;

		var pointer = JsonPointer.Parse(pointerString);

		var success = pointer.TryEvaluate(target, out var actual);

		Assert.IsFalse(success);
		Assert.IsNull(actual);
	}

	[Test]
	public void IndexingAnObjectInterpretsIndexAsKey()
	{
		using var target = JsonDocument.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"0\":false}}");

		var pointer = JsonPointer.Parse("/c/0");

		var actual = pointer.Evaluate(target.RootElement);

		// ReSharper disable once PossibleInvalidOperationException
		Assert.AreEqual(false, actual.Value.GetBoolean());
	}

	[Test]
	public void IndexingAnObjectInterpretsIndexAsKey_ButWithNodes()
	{
		var target = JsonNode.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"0\":false}}")!;

		var pointer = JsonPointer.Parse("/c/0");

		var success = pointer.TryEvaluate(target, out var actual);

		Assert.IsTrue(success);
		Assert.AreEqual(false, actual!.GetValue<bool>());
	}

	[Test]
	public void GettingLastItemInArray()
	{
		using var target = JsonDocument.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"0\":false}}");

		var pointer = JsonPointer.Parse("/b/-");

		var actual = pointer.Evaluate(target.RootElement);

		// ReSharper disable once PossibleInvalidOperationException
		Assert.AreEqual(JsonValueKind.Null, actual.Value.ValueKind);
	}

	[Test]
	public void GettingLastItemInArray_ButWithNodes()
	{
		var target = JsonNode.Parse("{\"a\":\"1\",\"b\":[5, true, null],\"c\":{\"0\":false}}")!;

		var pointer = JsonPointer.Parse("/b/-");

		var success = pointer.TryEvaluate(target, out var actual);

		Assert.IsTrue(success);
		Assert.IsNull(actual);
	}

	[Test]
	public void ImplicitCastTest()
	{
		var pointer = JsonPointer.Create("string", 1, "foo");

		Assert.AreEqual("/string/1/foo", pointer.ToString());
	}
}