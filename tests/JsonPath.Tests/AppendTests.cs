using System;
using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

public class AppendTests
{
	[Test]
	public void AppendSegment_Wildcard_PreservesSegmentAndEvaluates()
	{
		var wildcard = JsonPath.Parse("$[*]").Segments[0];
		var path = JsonPath.Root.Append("items").Append(wildcard);

		Assert.That(path.ToString(), Is.EqualTo("$['items'][*]"));

		var data = JsonNode.Parse("""{"items":[1,2,3]}""");
		var result = path.Evaluate(data);
		Assert.That(result.Matches.Select(m => m.Value!.GetValue<int>()), Is.EqualTo(new[] { 1, 2, 3 }));
	}

	[Test]
	public void AppendSegment_Filter_PreservesSegmentAndEvaluates()
	{
		var filter = JsonPath.Parse("$[?@ > 1]").Segments[0];
		var path = JsonPath.Root.Append("items").Append(filter);

		var data = JsonNode.Parse("""{"items":[1,2,3]}""");
		var result = path.Evaluate(data);
		Assert.That(result.Matches.Select(m => m.Value!.GetValue<int>()), Is.EqualTo(new[] { 2, 3 }));
	}

	[Test]
	public void AppendSegments_ReRootsNonSingularPath()
	{
		var source = JsonPath.Parse("$.view.payload.tags[*]");
		var relative = JsonPath.Root.Append(source.Segments.Skip(1));

		Assert.That(relative.ToString(), Is.EqualTo("$.payload.tags[*]"));

		var data = JsonNode.Parse("""{"payload":{"tags":["a","b"]}}""");
		var result = relative.Evaluate(data);
		Assert.That(result.Matches.Select(m => m.Value!.GetValue<string>()), Is.EqualTo(new[] { "a", "b" }));
	}

	[Test]
	public void AppendPath_CombinesSegments()
	{
		var prefix = JsonPath.Root.Append("view");
		var inner = JsonPath.Parse("$.payload.tags[*]");

		var combined = prefix.Append(inner);

		Assert.That(combined.ToString(), Is.EqualTo("$['view'].payload.tags[*]"));

		var data = JsonNode.Parse("""{"view":{"payload":{"tags":["x","y"]}}}""");
		var result = combined.Evaluate(data);
		Assert.That(result.Matches.Select(m => m.Value!.GetValue<string>()), Is.EqualTo(new[] { "x", "y" }));
	}

	[Test]
	public void AppendPath_Root_LeavesSegmentsUnchanged()
	{
		var path = JsonPath.Parse("$.foo.bar");

		var combined = path.Append(JsonPath.Root);

		Assert.That(combined.ToString(), Is.EqualTo("$.foo.bar"));
	}

	[Test]
	public void Append_NullArguments_ThrowArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => JsonPath.Root.Append((PathSegment)null!));
		Assert.Throws<ArgumentNullException>(() => JsonPath.Root.Append((System.Collections.Generic.IEnumerable<PathSegment>)null!));
		Assert.Throws<ArgumentNullException>(() => JsonPath.Root.Append((JsonPath)null!));
	}
}
