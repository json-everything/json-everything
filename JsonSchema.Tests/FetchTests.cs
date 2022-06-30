using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class FetchTests
{
	[Test]
	public void LocalRegistryFindsRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			SchemaRegistry =
			{
				Fetch = uri =>
				{
					if (uri.AbsoluteUri == "http://my.schema/test1")
						return JsonSchema.FromText("{\"type\": \"string\"}");
					return null;
				}
			}
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

		using var json = JsonDocument.Parse("10");

		var results = schema.Validate(json.RootElement, options);

		results.AssertInvalid();
	}
	[Test]
	public void GlobalRegistryFindsRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		SchemaRegistry.Global.Fetch = uri =>
		{
			if (uri.AbsoluteUri == "http://my.schema/test1")
				return JsonSchema.FromText("{\"type\": \"string\"}");
			return null;
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

		using var json = JsonDocument.Parse("10");

		var results = schema.Validate(json.RootElement, options);

		results.AssertInvalid();
	}
	[Test]
	public void LocalRegistryMissesRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			SchemaRegistry =
			{
				Fetch = uri =>
				{
					if (uri.AbsoluteUri == "http://my.schema/test2")
						return JsonSchema.FromText("{\"type\": \"string\"}");
					return null;
				}
			}
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

		using var json = JsonDocument.Parse("10");

		var results = schema.Validate(json.RootElement, options);

		results.AssertInvalid();
	}
	[Test]
	public void GlobalRegistryMissesRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		SchemaRegistry.Global.Fetch = uri =>
		{
			if (uri.AbsoluteUri == "http://my.schema/test2")
				return JsonSchema.FromText("{\"type\": \"string\"}");
			return null;
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

		using var json = JsonDocument.Parse("10");

		var results = schema.Validate(json.RootElement, options);

		results.AssertInvalid();
	}
}