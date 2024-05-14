using System;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

public class GithubTests
{
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==10)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==11)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==12)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==13)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==14)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==15)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==16)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==17)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==18)].doc_count")]
	[TestCase("$.aggregations.data.buckets[0].grade.buckets[?(@.key==19)].doc_count")]
	public void Issue150_ParseFailsWhenExpressionContainsLiteralNumberWith9(string pathText)
	{
		var path = JsonPath.Parse(pathText);
		Console.WriteLine(path);
	}

	[Test]
	public void Issue463_StringComparison()
	{
		var data = new JsonArray
		(
			"2023-04-23",
			"2023-06-07",
			"2023-07-08",
			"2023-08-09",
			"2023-09-10",
			"2024-01-01"
		);

		var path = JsonPath.Parse("$[?@ >= '2023-05-01']");
		var results = path.Evaluate(data);

		Assert.AreEqual(5, results.Matches!.Count);
	}

	[Test]
	public void Issue495_TryParseEmptyString()
	{
		var success = JsonPath.TryParse(string.Empty, out var path);

		Assert.False(success);
		Assert.Null(path);
	}
}