using System;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class AnchorTests
{
	[TestCase("inner:colon", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("under_score", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("inner-hyphen", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("inner.period", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("start0123456789end", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("_underscore", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("under_score", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("inner-hyphen", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("inner.period", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("start0123456789end", "https://json-schema.org/draft/2020-12/schema")]
	public void ValidAnchor(string anchor, string metaSchemaUri)
	{
		var schema = JsonSchema.FromText(
			$$"""
			  {
			    "$schema": "{{metaSchemaUri}}",
			    "type": "object",
			    "properties": {
			      "foo": { "$ref": "#{{anchor}}" }
			    },
			    "$defs": {
			      "foo-def": {
			        "$anchor": "{{anchor}}",
			        "type": "string"
			      }
			    }
			  }
			  """
		);

		var instance = JsonDocument.Parse("""{ "foo": "value" }""").RootElement;

		var results = schema.Evaluate(instance);

		results.AssertValid();
	}

	[TestCase("#foo", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("0number", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase(".period", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("-hyphen", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("_underscore", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase(":colon", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("/a/b", "https://json-schema.org/draft/2019-09/schema")]
	[TestCase("#foo", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase(".period", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("-hyphen", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase(":colon", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("inner:colon", "https://json-schema.org/draft/2020-12/schema")]
	[TestCase("/a/b", "https://json-schema.org/draft/2020-12/schema")]
	public void InvalidAnchor(string anchor, string metaSchemaUri)
	{
		JsonSchema schema;
		try
		{
			schema = JsonSchema.FromText(
				$$"""
				  {
				    "$schema": "{{metaSchemaUri}}",
				    "type": "object",
				    "properties": {
				      "foo": { "$ref": "#{{anchor}}" }
				    },
				    "$defs": {
				      "foo-def": {
				        "$anchor": "{{anchor}}",
				        "type": "string"
				      }
				    }
				  }
				  """
			);
		}
		catch (Exception e)
		{
			TestConsole.WriteLine("failed during deserialization");
			TestConsole.WriteLine(e);
			return;
		}

		var instance = JsonDocument.Parse("""{ "foo": "value" }""").RootElement;

		EvaluationResults results;
		try
		{
			results = schema.Evaluate(instance);
		}
		catch (Exception e)
		{
			TestConsole.WriteLine("failed during evaluation");
			TestConsole.WriteLine(e);
			return;
		}

		results.AssertInvalid();
	}
}