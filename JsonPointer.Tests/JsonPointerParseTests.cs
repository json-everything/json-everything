using System.Collections;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Json.Pointer.Tests
{
	[TestFixture]
	public class JsonPointerParseTests
	{
		public static IEnumerable SpecificationExamples
		{
			get
			{
				// Normal
				yield return new TestCaseData("", new string[] { });
				yield return new TestCaseData("/foo", new[] {"foo"});
				yield return new TestCaseData("/foo/0", new[] {"foo", "0"});
				yield return new TestCaseData("/", new[] {""});
				yield return new TestCaseData("/a~1b", new[] {"a/b"});
				yield return new TestCaseData("/c%d", new[] {"c%d"});
				yield return new TestCaseData("/e^f", new[] {"e^f"});
				yield return new TestCaseData("/g|h", new[] {"g|h"});
				yield return new TestCaseData("/i\\j", new[] {"i\\j"});
				yield return new TestCaseData("/k\"l", new[] {"k\"l"});
				yield return new TestCaseData("/ ", new[] {" "});
				yield return new TestCaseData("/m~0n", new[] {"m~n"});
				yield return new TestCaseData("/c%25d", new[] { "c%25d" });
				yield return new TestCaseData("/e%5Ef", new[] { "e%5Ef" });
				yield return new TestCaseData("/g%7Ch", new[] { "g%7Ch" });
				yield return new TestCaseData("/i%5Cj", new[] { "i%5Cj" });
				yield return new TestCaseData("/k%22l", new[] { "k%22l" });
				yield return new TestCaseData("/%20", new[] { "%20" });
				// Url
				yield return new TestCaseData("#", new string[] { });
				yield return new TestCaseData("#/foo", new[] {"foo"});
				yield return new TestCaseData("#/foo/0", new[] {"foo", "0"});
				yield return new TestCaseData("#/", new[] {""});
				yield return new TestCaseData("#/a~1b", new[] {"a/b"});
				yield return new TestCaseData("#/c%25d", new[] {"c%d"});
				yield return new TestCaseData("#/e%5Ef", new[] {"e^f"});
				yield return new TestCaseData("#/g%7Ch", new[] {"g|h"});
				yield return new TestCaseData("#/i%5Cj", new[] {"i\\j"});
				yield return new TestCaseData("#/k%22l", new[] {"k\"l"});
				yield return new TestCaseData("#/%20", new[] {" "});
				yield return new TestCaseData("#/m~0n", new[] {"m~n"});
			}
		}
		public static IEnumerable FailureCases
		{
			get
			{
				// Normal
				yield return new TestCaseData("starts/with/segment");
				yield return new TestCaseData("/invalid/escap~e/sequence");
				yield return new TestCaseData("/ends/with~");
				// Url
				yield return new TestCaseData("#uses/anchor/name");
				yield return new TestCaseData("#/invalid/escap~e/sequence");
				yield return new TestCaseData("#/ends/with~");
				yield return new TestCaseData("#/invalid/hex/%2h");
				yield return new TestCaseData("#/short/%2/hex");
				yield return new TestCaseData("#/end/short/hex/%5");
				yield return new TestCaseData("#/missing/value/%/hex");
				yield return new TestCaseData("#/end/missing/value/hex/%");
			}
		}

		[TestCaseSource(nameof(SpecificationExamples))]
		public void Parse(string pointerString, string[] segments)
		{
			var pointer = JsonPointer.Parse(pointerString);

			pointer.Segments.Length.Should().Be(segments.Length);
			pointer.Segments.Select(s => s.Value).Should().BeEquivalentTo(segments);
		}

		[TestCaseSource(nameof(SpecificationExamples))]
		public void TryParse(string pointerString, string[] segments)
		{
			Assert.IsTrue(JsonPointer.TryParse(pointerString, out var pointer));

			pointer.Segments.Length.Should().Be(segments.Length);
			pointer.Segments.Select(s => s.Value).Should().BeEquivalentTo(segments);
		}

		[TestCaseSource(nameof(FailureCases))]
		public void ParseFailure(string pointerString)
		{
			Assert.Throws<PointerParseException>(() => JsonPointer.Parse(pointerString));
		}

		[TestCaseSource(nameof(FailureCases))]
		public void TryParseFailure(string pointerString)
		{
			Assert.False(JsonPointer.TryParse(pointerString, out _));
		}

		[Test]
		public void ParseExpectPlainGetUriEncoded()
		{
			Assert.Throws<PointerParseException>(() =>
			{
				JsonPointer.Parse("#/one/2/three", JsonPointerKind.Plain);
			});
		}

		[Test]
		public void ParseExpectUriEncodedGetPlain()
		{
			Assert.Throws<PointerParseException>(() =>
			{
				JsonPointer.Parse("/one/2/three", JsonPointerKind.UriEncoded);
			});
		}

		[Test]
		public void TryParseExpectPlainGetUriEncoded()
		{
			Assert.IsFalse(JsonPointer.TryParse("#/one/2/three", out _, JsonPointerKind.Plain));
		}

		[Test]
		public void TryParseExpectUriEncodedGetPlain()
		{
			Assert.IsFalse(JsonPointer.TryParse("/one/2/three", out _, JsonPointerKind.UriEncoded));
		}
	}
}
