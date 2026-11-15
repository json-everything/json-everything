using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;
using TestHelpers;

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
		TestConsole.WriteLine(path);
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

		Assert.That(results.Matches!, Has.Count.EqualTo(5));
	}

	[Test]
	public void Issue495_TryParseEmptyString()
	{
		var success = JsonPath.TryParse(string.Empty, out var path);

		Assert.Multiple(() =>
		{
			Assert.That(success, Is.False);
			Assert.That(path, Is.Null);
		});
	}

	[Test]
	public void Issue787_DotSyntaxWithoutNameThrowsInTryParse()
	{
		var pathText = "$.";

		Assert.That(JsonPath.TryParse(pathText, out _), Is.False);
	}

	[Test]
	public void Issue980_PathOnRightOfExpressionOperator()
	{
		var jsonDocument = new JsonObject
		{
			["data"] = new JsonArray
			{
				new JsonObject
				{
					["elements"] = new JsonArray
					{
						new JsonObject
						{
							["name"] = "ID",
							["value"] = "TestValue"
						}
					}
				}
			},
			["name"] = new JsonObject
			{
				["id"] = "ID"
			}
		};

		var expectedResult = new JsonArray
		{
			new JsonObject
			{
				["elements"] = new JsonArray
				{
					new JsonObject
					{
						["name"] = "ID",
						["value"] = "TestValue"
					}
				}
			}
		};

		var path = JsonPath.Parse("$.data[?@.elements[?@.name == $.name.id]]");
		var result = path.Evaluate(jsonDocument);
		var matches = new JsonArray([.. from match in result.Matches select match.Value?.DeepClone()]);
		Assert.That(expectedResult.ToJsonString(), Is.EqualTo(matches.ToJsonString()));
	}

	[TestCase("$[\"\\u0041\\t\"]", "A\t")]
	[TestCase("$[\"\\u0041\\\"\"]", "A\"")]
	[TestCase("$[\"\\u0041\\\\\"]", "A\\")]
	[TestCase("$[\"\\u0041\\u0042\\t\"]", "AB\t")]
	[TestCase("$[\"\\uD83D\\uDE00\"]", "😀")]
	[TestCase("$[\"\\uD83D\\uDE00\\\"\"]", "😀\"")]
	public void Issue1073_UnicodeEscapeFollowedByAnotherEscape(string pathText, string expectedName)
	{
		var path = JsonPath.Parse(pathText);
		var selector = path.Segments[0].Selectors[0];

		Assert.That(selector, Is.TypeOf<NameSelector>());
		Assert.That(((NameSelector)selector).Name, Is.EqualTo(expectedName));
	}

}