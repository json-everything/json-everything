using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class AnchorTests
{
	[TestCase("inner:colon", MetaSchemas.Draft201909IdValue)]
	[TestCase("under_score", MetaSchemas.Draft201909IdValue)]
	[TestCase("inner-hyphen", MetaSchemas.Draft201909IdValue)]
	[TestCase("inner.period", MetaSchemas.Draft201909IdValue)]
	[TestCase("start0123456789end", MetaSchemas.Draft201909IdValue)]
	[TestCase("_underscore", MetaSchemas.Draft202012IdValue)]
	[TestCase("under_score", MetaSchemas.Draft202012IdValue)]
	[TestCase("inner-hyphen", MetaSchemas.Draft202012IdValue)]
	[TestCase("inner.period", MetaSchemas.Draft202012IdValue)]
	[TestCase("start0123456789end", MetaSchemas.Draft202012IdValue)]
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

		var instance = new JsonObject { ["foo"] = "value" };

		var results = schema.Evaluate(instance);

		results.AssertValid();
	}

	[TestCase("#foo", MetaSchemas.Draft201909IdValue)]
	[TestCase("0number", MetaSchemas.Draft201909IdValue)]
	[TestCase(".period", MetaSchemas.Draft201909IdValue)]
	[TestCase("-hyphen", MetaSchemas.Draft201909IdValue)]
	[TestCase("_underscore", MetaSchemas.Draft201909IdValue)]
	[TestCase(":colon", MetaSchemas.Draft201909IdValue)]
	[TestCase("/a/b", MetaSchemas.Draft201909IdValue)]
	[TestCase("#foo", MetaSchemas.Draft202012IdValue)]
	[TestCase(".period", MetaSchemas.Draft202012IdValue)]
	[TestCase("-hyphen", MetaSchemas.Draft202012IdValue)]
	[TestCase(":colon", MetaSchemas.Draft202012IdValue)]
	[TestCase("inner:colon", MetaSchemas.Draft202012IdValue)]
	[TestCase("/a/b", MetaSchemas.Draft202012IdValue)]
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
			Console.WriteLine("failed during deserialization");
			Console.WriteLine(e);
			return;
		}

		var instance = new JsonObject { ["foo"] = "value" };

		EvaluationResults results;
		try
		{
			results = schema.Evaluate(instance);
		}
		catch (Exception e)
		{
			Console.WriteLine("failed during evaluation");
			Console.WriteLine(e);
			return;
		}

		results.AssertInvalid();
	}
}