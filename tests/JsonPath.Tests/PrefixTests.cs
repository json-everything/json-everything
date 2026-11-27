using System;
using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

public class PrefixTests
{
	[TestCase("$.view.payload.name", "$", true)]
	[TestCase("$.view.payload.name", "$.view", true)]
	[TestCase("$.view.payload.name", "$['view']", true)]
	[TestCase("$.view.payload.name", "$.view.payload", true)]
	[TestCase("$.view.payload.name", "$.view.payload.name", true)]
	[TestCase("$.view.payload.tags[*]", "$.view", true)]
	[TestCase("$.view", "$.view.payload", false)]
	[TestCase("$.view.meta.nulls", "$.view.payload", false)]
	[TestCase("$.embeddings.title", "$.view", false)]
	[TestCase("$..view.payload", "$.view", false)]
	public void StartsWith_ReturnsExpectedResult(string pathText, string prefixText, bool expected)
	{
		var path = JsonPath.Parse(pathText);
		var prefix = JsonPath.Parse(prefixText);

		Assert.That(path.StartsWith(prefix), Is.EqualTo(expected));
	}

	[TestCase("$.view.payload.name", "$", "$.view.payload.name")]
	[TestCase("$.view.payload.name", "$.view", "$.payload.name")]
	[TestCase("$.view.payload.name", "$['view']", "$.payload.name")]
	[TestCase("$.view.payload.tags[*]", "$.view", "$.payload.tags[*]")]
	[TestCase("$.view.payload.name", "$.view.payload.name", "$")]
	[TestCase("$.view.meta.nulls", "$.view.payload", "$.view.meta.nulls")]
	[TestCase("$.embeddings.title", "$.view", "$.embeddings.title")]
	public void TrimStart_ReturnsExpectedPath(string pathText, string prefixText, string expectedText)
	{
		var path = JsonPath.Parse(pathText);
		var prefix = JsonPath.Parse(prefixText);
		var expected = JsonPath.Parse(expectedText);

		var trimmed = path.TrimStart(prefix);

		Assert.That(trimmed, Is.EqualTo(expected));
	}

	[Test]
	public void TryTrimStart_WhenMatching_ReturnsTrueAndEvaluatesAgainstSubtree()
	{
		var fullPath = JsonPath.Parse("$.view.payload.tags[*]");
		var prefix = JsonPath.Root.Append("view");

		var matched = fullPath.TryTrimStart(prefix, out var relative);

		Assert.That(matched, Is.True);
		Assert.That(relative, Is.EqualTo(JsonPath.Parse("$.payload.tags[*]")));

		var subtree = JsonNode.Parse("""{"payload":{"tags":["a","b"]}}""");
		var result = relative!.Evaluate(subtree);
		Assert.That(result.Matches.Select(m => m.Value!.GetValue<string>()), Is.EqualTo(new[] { "a", "b" }));
	}

	[Test]
	public void TryTrimStart_WhenNotMatching_ReturnsFalseAndNull()
	{
		var path = JsonPath.Parse("$.view.meta.nulls");
		var prefix = JsonPath.Parse("$.view.payload");

		var matched = path.TryTrimStart(prefix, out var relative);

		Assert.That(matched, Is.False);
		Assert.That(relative, Is.Null);
	}

	[Test]
	public void PrefixMethods_NullPrefix_ThrowArgumentNullException()
	{
		var path = JsonPath.Parse("$.view");

		Assert.Throws<ArgumentNullException>(() => path.StartsWith(null!));
		Assert.Throws<ArgumentNullException>(() => path.TrimStart(null!));
		Assert.Throws<ArgumentNullException>(() => path.TryTrimStart(null!, out _));
	}
}
