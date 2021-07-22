using System.Collections;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Pointer.Tests
{
	[TestFixture]
	public class SpecificationTests
	{
		public static IEnumerable ExampleCases
		{
			get
			{
				// Basic
				yield return new TestCaseData("", @"{
					""foo"": [""bar"", ""baz""],
					"""": 0,
					""a/b"": 1,
					""c%d"": 2,
					""e^f"": 3,
					""g|h"": 4,
					""i\\j"": 5,
					""k\""l"": 6,
					"" "": 7,
					""m~n"": 8
				}");
				yield return new TestCaseData("/foo", @"[""bar"", ""baz""]");
				yield return new TestCaseData("/foo/0", "\"bar\"");
				yield return new TestCaseData("/", "0");
				yield return new TestCaseData("/a~1b", "1");
				yield return new TestCaseData("/c%d", "2");
				yield return new TestCaseData("/e^f", "3");
				yield return new TestCaseData("/g|h", "4");
				yield return new TestCaseData("/i\\j", "5");
				yield return new TestCaseData("/k\"l", "6");
				yield return new TestCaseData("/ ", "7");
				yield return new TestCaseData("/m~0n", "8");
				// Url
				yield return new TestCaseData("#", @"{
					""foo"": [""bar"", ""baz""],
					"""": 0,
					""a/b"": 1,
					""c%d"": 2,
					""e^f"": 3,
					""g|h"": 4,
					""i\\j"": 5,
					""k\""l"": 6,
					"" "": 7,
					""m~n"": 8
				}");
				yield return new TestCaseData("#/foo", @"[""bar"", ""baz""]");
				yield return new TestCaseData("#/foo/0", "\"bar\"");
				yield return new TestCaseData("#/", "0");
				yield return new TestCaseData("#/a~1b", "1");
				yield return new TestCaseData("#/c%25d", "2");
				yield return new TestCaseData("#/e%5Ef", "3");
				yield return new TestCaseData("#/g%7Ch", "4");
				yield return new TestCaseData("#/i%5Cj", "5");
				yield return new TestCaseData("#/k%22l", "6");
				yield return new TestCaseData("#/%20", "7");
				yield return new TestCaseData("#/m~0n", "8");
			}
		}

		[TestCaseSource(nameof(ExampleCases))]
		public void Example(string pointerString, string expectedString)
		{
			using var target = JsonDocument.Parse(@"{
				""foo"": [""bar"", ""baz""],
				"""": 0,
				""a/b"": 1,
				""c%d"": 2,
				""e^f"": 3,
				""g|h"": 4,
				""i\\j"": 5,
				""k\""l"": 6,
				"" "": 7,
				""m~n"": 8
			}");

			var pointer = JsonPointer.Parse(pointerString);

			var actual = pointer.Evaluate(target.RootElement);

			using var expected = JsonDocument.Parse(expectedString);

			// ReSharper disable once PossibleInvalidOperationException
			Assert.IsTrue(actual.Value.IsEquivalentTo(expected.RootElement));
		}
	}
}
