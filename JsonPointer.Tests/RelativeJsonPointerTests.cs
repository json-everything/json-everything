using System.Collections;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Pointer.Tests
{
	[TestFixture]
	public class RelativeJsonPointerTests
	{
		public static IEnumerable SpecificationExamples
		{
			get
			{
				yield return new TestCaseData("0", "\"baz\"");
				yield return new TestCaseData("1/0", "\"bar\"");
				yield return new TestCaseData("0-1", "\"bar\"");
				yield return new TestCaseData("2/highly/nested/objects", "true");
				yield return new TestCaseData("0#", "1");
				yield return new TestCaseData("0-1#", "0");
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
		[Ignore("Not supported by System.Text.Json (see https://github.com/dotnet/runtime/issues/40452)")]
		public void EvaluateSuccess(string pointerString, string expectedString)
		{
			using var json = JsonDocument.Parse("{\"foo\":[\"bar\",\"baz\"],\"highly\":{\"nested\":{\"objects\":true}}}");
			var startElement = json.RootElement.GetProperty("foo")[1];

			var pointer = RelativeJsonPointer.Parse(pointerString);
			using var expected = JsonDocument.Parse(expectedString);
 
#pragma warning disable 618
			var actual = pointer.Evaluate(startElement);
#pragma warning restore 618

			Assert.True(actual.IsEquivalentTo(expected.RootElement));
		}

		[TestCaseSource(nameof(FailureCases))]
		public void EvaluateFailure(string pointerString)
		{
			Assert.Throws<PointerParseException>(() => RelativeJsonPointer.Parse(pointerString));
		}
	}
}