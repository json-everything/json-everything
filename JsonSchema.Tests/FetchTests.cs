using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

#pragma warning disable CS1998

namespace Json.Schema.Tests;

public class FetchTests
{
	[Test]
	public async Task LocalRegistryFindsRef()
	{
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			SchemaRegistry =
			{
				Fetch = async uri =>
				{
					if (uri.AbsoluteUri == "http://my.schema/test1")
						return JsonSchema.FromText("{\"type\": \"string\"}");
					return null;
				}
			}
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

		using var json = JsonDocument.Parse("10");

		var results = await schema.Evaluate(json.RootElement, options);

		results.AssertInvalid();
	}

	[Test]
	public async Task GlobalRegistryFindsRef()
	{
		try
		{
			var options = new EvaluationOptions
			{
				OutputFormat = OutputFormat.Hierarchical
			};
			SchemaRegistry.Global.Fetch = async uri =>
			{
				if (uri.AbsoluteUri == "http://my.schema/test1")
					return JsonSchema.FromText("{\"type\": \"string\"}");
				return null;
			};
			var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

			using var json = JsonDocument.Parse("10");

			var results = await schema.Evaluate(json.RootElement, options);

			results.AssertInvalid();
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}

	[Test]
	public void LocalRegistryMissesRef()
	{
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			SchemaRegistry =
			{
				Fetch = async uri =>
				{
					if (uri.AbsoluteUri == "http://my.schema/test2")
						return JsonSchema.FromText("{\"type\": \"string\"}");
					return null;
				}
			}
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

		using var json = JsonDocument.Parse("10");

		Assert.ThrowsAsync<JsonSchemaException>(() => schema.Evaluate(json.RootElement, options));
	}

	[Test]
	public void GlobalRegistryMissesRef()
	{
		try
		{
			var options = new EvaluationOptions
			{
				OutputFormat = OutputFormat.Hierarchical
			};
			SchemaRegistry.Global.Fetch = async uri =>
			{
				if (uri.AbsoluteUri == "http://my.schema/test2")
					return JsonSchema.FromText("{\"type\": \"string\"}");
				return null;
			};
			var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

			using var json = JsonDocument.Parse("10");

			Assert.ThrowsAsync<JsonSchemaException>(() => schema.Evaluate(json.RootElement, options));
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}

	[Test]
	public void RefContainsBadJson()
	{
		try
		{
			SchemaRegistry.Global.Fetch = async _ => JsonSchema.FromText("{\"type\": \"string\", \"invalid\"}");
			var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

			using var json = JsonDocument.Parse("10");

			Assert.ThrowsAsync<JsonException>(() => schema.Evaluate(json.RootElement));
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}
}