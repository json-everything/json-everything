using System.Collections;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Json.Pointer.Tests;

[TestFixture]
public class JsonPointerParseTests
{
	public static IEnumerable SpecificationExamples
	{
		get
		{
			yield return new TestCaseData("", new string[] { });
			yield return new TestCaseData("/foo", new[] { "foo" });
			yield return new TestCaseData("/foo/0", new[] { "foo", "0" });
			yield return new TestCaseData("/", new[] { "" });
			yield return new TestCaseData("/a~1b", new[] { "a/b" });
			yield return new TestCaseData("/c%d", new[] { "c%d" });
			yield return new TestCaseData("/e^f", new[] { "e^f" });
			yield return new TestCaseData("/g|h", new[] { "g|h" });
			yield return new TestCaseData("/i\\j", new[] { "i\\j" });
			yield return new TestCaseData("/k\"l", new[] { "k\"l" });
			yield return new TestCaseData("/ ", new[] { " " });
			yield return new TestCaseData("/m~0n", new[] { "m~n" });
			yield return new TestCaseData("/c%25d", new[] { "c%25d" });
			yield return new TestCaseData("/e%5Ef", new[] { "e%5Ef" });
			yield return new TestCaseData("/g%7Ch", new[] { "g%7Ch" });
			yield return new TestCaseData("/i%5Cj", new[] { "i%5Cj" });
			yield return new TestCaseData("/k%22l", new[] { "k%22l" });
			yield return new TestCaseData("/%20", new[] { "%20" });
		}
	}
	public static IEnumerable UrlSpecificationExamples
	{
		get
		{
			yield return new TestCaseData("#", new string[] { });
			yield return new TestCaseData("#/foo", new[] { "foo" });
			yield return new TestCaseData("#/foo/0", new[] { "foo", "0" });
			yield return new TestCaseData("#/", new[] { "" });
			yield return new TestCaseData("#/a~1b", new[] { "a/b" });
			yield return new TestCaseData("#/c%25d", new[] { "c%d" });
			yield return new TestCaseData("#/e%5Ef", new[] { "e^f" });
			yield return new TestCaseData("#/g%7Ch", new[] { "g|h" });
			yield return new TestCaseData("#/i%5Cj", new[] { "i\\j" });
			yield return new TestCaseData("#/k%22l", new[] { "k\"l" });
			yield return new TestCaseData("#/%20", new[] { " " });
			yield return new TestCaseData("#/m~0n", new[] { "m~n" });
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
		}
	}

	[TestCaseSource(nameof(SpecificationExamples))]
	[TestCaseSource(nameof(UrlSpecificationExamples))]
	public void Parse(string pointerString, string[] segments)
	{
		var pointer = JsonPointer.Parse(pointerString);

		pointer.Count.Should().Be(segments.Length);
		for (int i = 0; i < pointer.Count; i++)
		{
			var segment = pointer[i];
			var expected = segments[i];

			Assert.That(segment, Is.EqualTo(expected));
		}
	}

	[TestCaseSource(nameof(SpecificationExamples))]
	public void TryParse(string pointerString, string[] segments)
	{
		Assert.That(JsonPointer.TryParse(pointerString, out var pointer), Is.True);

		pointer!.Count.Should().Be(segments.Length);
		for (int i = 0; i < pointer.Count; i++)
		{
			var segment = pointer[i];
			var expected = segments[i];

			Assert.That(segment, Is.EqualTo(expected));
		}
	}

	[TestCaseSource(nameof(FailureCases))]
	public void ParseFailure(string pointerString)
	{
		Assert.Throws<PointerParseException>(() => JsonPointer.Parse(pointerString));
	}

	[TestCaseSource(nameof(FailureCases))]
	public void TryParseFailure(string pointerString)
	{
		Assert.That(JsonPointer.TryParse(pointerString, out _), Is.False);
	}

	[Test]
	public void ParseShouldStoreNonUrlForm()
	{
		var pointer = JsonPointer.Parse("#/foo");
		var expected = "/foo";

		var actual = pointer.ToString();

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TryParseShouldStoreNonUrlForm()
	{
		Assert.That(JsonPointer.TryParse("#/foo", out var pointer), Is.True);
		var expected = "/foo";

		var actual = pointer!.ToString();

		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCaseSource(nameof(SpecificationExamples))]
	public void CreateThenToString(string pointerString, string[] segments)
	{
		var pointer = JsonPointer.Create(segments.Select(x => (PointerSegment)x).ToArray());

		var backToString = pointer.ToString();
		Assert.That(backToString, Is.EqualTo(pointerString));
	}
}