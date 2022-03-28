using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Pointer.Tests;

[TestFixture]
public class RelativeJsonPointerTests
{
	public static IEnumerable SpecificationExamples
	{
		get
		{
			yield return new TestCaseData("0", "\"baz\"");
			yield return new TestCaseData("1/0", "\"bar\"");
			//yield return new TestCaseData("0-1", "\"bar\"");
			yield return new TestCaseData("2/highly/nested/objects", "true");
			yield return new TestCaseData("0#", "1");
			//yield return new TestCaseData("0-1#", "0");
			yield return new TestCaseData("1#", "\"foo\"");
		}
	}
	public static IEnumerable FailureCases
	{
		get
		{
			yield return new TestCaseData("#/");
			yield return new TestCaseData("#/end");
			yield return new TestCaseData("5#/end");
			yield return new TestCaseData("/end");
			yield return new TestCaseData("-1/end");
			yield return new TestCaseData("end");
		}
	}

	[TestCaseSource(nameof(SpecificationExamples))]
	public void EvaluateSuccess(string pointerString, string expectedString)
	{
		var json = JsonNode.Parse("{\"foo\":[\"bar\",\"baz\"],\"highly\":{\"nested\":{\"objects\":true}}}")!;
		var startElement = json["foo"]![1]!;

		var pointer = RelativeJsonPointer.Parse(pointerString);
		var expected = JsonNode.Parse(expectedString)!;

		var success = pointer.TryEvaluate(startElement, out var actual);

		Assert.IsTrue(success);
		Assert.IsTrue(actual!.IsEquivalentTo(expected));
	}

	[TestCaseSource(nameof(FailureCases))]
	public void EvaluateFailure(string pointerString)
	{
		Assert.Throws<PointerParseException>(() => RelativeJsonPointer.Parse(pointerString));
	}
}