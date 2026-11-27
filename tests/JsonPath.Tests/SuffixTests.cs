using System;
using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

public class SuffixTests
{
	[TestCase("$.view.payload.name", "$", true)]
	[TestCase("$.view.payload.name", "$.name", true)]
	[TestCase("$.view.payload.name", "$['name']", true)]
	[TestCase("$.view.payload.name", "$.payload.name", true)]
	[TestCase("$.view.payload.name", "$.view.payload.name", true)]
	[TestCase("$.view.payload.tags[*]", "$.tags[*]", true)]
	[TestCase("$.view", "$.view.payload", false)]
	[TestCase("$.view.meta.nulls", "$.payload.nulls", false)]
	[TestCase("$.embeddings.title", "$.name", false)]
	[TestCase("$.view..name", "$.name", false)]
	[TestCase("$.view..name", "$..name", true)]
	public void EndsWith_ReturnsExpectedResult(string pathText, string suffixText, bool expected)
	{
		var path = JsonPath.Parse(pathText);
		var suffix = JsonPath.Parse(suffixText);

		Assert.That(path.EndsWith(suffix), Is.EqualTo(expected));
	}

	[TestCase("$.view.payload.name", "$", "$.view.payload.name")]
	[TestCase("$.view.payload.name", "$.name", "$.view.payload")]
	[TestCase("$.view.payload.name", "$['name']", "$.view.payload")]
	[TestCase("$.view.payload.tags[*]", "$[*]", "$.view.payload.tags")]
	[TestCase("$.view.payload.name", "$.view.payload.name", "$")]
	[TestCase("$.view.meta.nulls", "$.payload.nulls", "$.view.meta.nulls")]
	[TestCase("$.embeddings.title", "$.name", "$.embeddings.title")]
	public void TrimEnd_ReturnsExpectedPath(string pathText, string suffixText, string expectedText)
	{
		var path = JsonPath.Parse(pathText);
		var suffix = JsonPath.Parse(suffixText);
		var expected = JsonPath.Parse(expectedText);

		var trimmed = path.TrimEnd(suffix);

		Assert.That(trimmed, Is.EqualTo(expected));
	}

	[Test]
	public void TryTrimEnd_WhenMatching_ReturnsTrueAndEvaluatesAgainstParent()
	{
		var fullPath = JsonPath.Parse("$.view.payload.tags[*]");
		var suffix = JsonPath.Parse("$.tags[*]");

		var matched = fullPath.TryTrimEnd(suffix, out var parent);

		Assert.That(matched, Is.True);
		Assert.That(parent, Is.EqualTo(JsonPath.Parse("$.view.payload")));

		var doc = JsonNode.Parse("""{"view":{"payload":{"tags":["a","b"]}}}""");
		var result = parent!.Evaluate(doc);
		Assert.That(result.Matches.Count, Is.EqualTo(1));
		Assert.That(result.Matches[0].Value!["tags"]!.AsArray().Select(n => n!.GetValue<string>()), Is.EqualTo(new[] { "a", "b" }));
	}

	[Test]
	public void TryTrimEnd_WhenNotMatching_ReturnsFalseAndNull()
	{
		var path = JsonPath.Parse("$.view.meta.nulls");
		var suffix = JsonPath.Parse("$.payload.nulls");

		var matched = path.TryTrimEnd(suffix, out var parent);

		Assert.That(matched, Is.False);
		Assert.That(parent, Is.Null);
	}

	[Test]
	public void SuffixMethods_NullSuffix_ThrowArgumentNullException()
	{
		var path = JsonPath.Parse("$.view");

		Assert.Throws<ArgumentNullException>(() => path.EndsWith(null!));
		Assert.Throws<ArgumentNullException>(() => path.TrimEnd(null!));
		Assert.Throws<ArgumentNullException>(() => path.TryTrimEnd(null!, out _));
	}
}
