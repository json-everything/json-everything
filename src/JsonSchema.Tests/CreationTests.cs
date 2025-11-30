using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class CreationTests
{
	[Test]
	public void FromText()
	{
		var schema = JsonSchema.FromText("{\"$id\":\"https://json-everything.test/FromText\",\"minimum\":5}");

		var results = schema.Evaluate(10.AsJsonElement());

		results.AssertValid();
	}

	[Test]
	public void FromTextIgnoringComments()
	{
		var options = new JsonDocumentOptions
		{
			CommentHandling = JsonCommentHandling.Skip
		};
		var schema = JsonSchema.FromText(
			"""
			{
			  "$id":"https://json-everything.test/FromTextIgnoringComments",
			  // comment here, just passing through
			  "minimum":5
			}
			""", jsonOptions: options);

		using var json = JsonDocument.Parse("10");

		var results = schema.Evaluate(json.RootElement);

		results.AssertValid();
	}
}