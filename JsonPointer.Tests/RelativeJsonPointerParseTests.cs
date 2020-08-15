using System.Collections;
using System.Linq;
using FluentAssertions;
using Json.Pointer;
using NUnit.Framework;

namespace JsonPointer.Tests
{
	[TestFixture]
	public class RelativeJsonPointerParseTests
	{
		public static IEnumerable SpecificationExamples
		{
			get
			{
				yield return new TestCaseData("1/foo", 1, new[] {"foo"});
				yield return new TestCaseData("2/foo/0", 2, new[] {"foo", "0"});
				yield return new TestCaseData("3/", 3, new[] {""});
				yield return new TestCaseData("4/a~1b", 4, new[] {"a/b"});
				yield return new TestCaseData("5/c%d", 5, new[] {"c%d"});
				yield return new TestCaseData("6/e^f", 6, new[] {"e^f"});
				yield return new TestCaseData("7/g|h", 7, new[] {"g|h"});
				yield return new TestCaseData("8/i\\j", 8, new[] {"i\\j"});
				yield return new TestCaseData("9/k\"l", 9, new[] {"k\"l"});
				yield return new TestCaseData("10/ ", 10, new[] {" "});
				yield return new TestCaseData("11/m~0n", 11, new[] {"m~n"});
				yield return new TestCaseData("12/c%25d", 12, new[] {"c%25d"});
				yield return new TestCaseData("13/e%5Ef", 13, new[] {"e%5Ef"});
				yield return new TestCaseData("14/g%7Ch", 14, new[] {"g%7Ch"});
				yield return new TestCaseData("15/i%5Cj", 15, new[] {"i%5Cj"});
				yield return new TestCaseData("16/k%22l", 16, new[] {"k%22l"});
				yield return new TestCaseData("17/%20", 17, new[] {"%20"});
				yield return new TestCaseData("0", 0, new string[] { });
				yield return new TestCaseData("1/0", 1, new[] {"0"});
				yield return new TestCaseData("2/highly/nested/objects", 2, new[] {"highly", "nested", "objects"});
				yield return new TestCaseData("0#", 0, new string[] { });
				yield return new TestCaseData("1#", 1, new string[] { });
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
		public void Parse(string pointerString, int parentSteps, string[] segments)
		{
			var pointer = RelativeJsonPointer.Parse(pointerString);

			pointer.ParentSteps.Should().Be((uint) parentSteps);
			pointer.Pointer.Segments.Length.Should().Be(segments.Length);
			pointer.Pointer.Segments.Select(s => s.Value).Should().BeEquivalentTo(segments);
		}

		[TestCaseSource(nameof(SpecificationExamples))]
		public void TryParse(string pointerString, int parentSteps, string[] segments)
		{
			Assert.IsTrue(RelativeJsonPointer.TryParse(pointerString, out var pointer));

			pointer.ParentSteps.Should().Be((uint) parentSteps);
			pointer.Pointer.Segments.Length.Should().Be(segments.Length);
			pointer.Pointer.Segments.Select(s => s.Value).Should().BeEquivalentTo(segments);
		}

		[TestCaseSource(nameof(FailureCases))]
		public void ParseFailure(string pointerString)
		{
			Assert.Throws<PointerParseException>(() => RelativeJsonPointer.Parse(pointerString));
		}

		[TestCaseSource(nameof(FailureCases))]
		public void TryParseFailure(string pointerString)
		{
			Assert.False(RelativeJsonPointer.TryParse(pointerString, out _));
		}
	}
}
