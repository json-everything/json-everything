using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class FetchTests
{
	[Test]
	public void LocalRegistryFindsRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Detailed,
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
		results.SchemaLocation.Segments.Last().Value.Should().NotBe("$ref");
	}

	[Test]
	public void GlobalRegistryFindsRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Detailed
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
		results.SchemaLocation.Segments.Last().Value.Should().NotBe("$ref");
	}

	[Test]
	public void LocalRegistryMissesRef()
	{
		var options = new ValidationOptions
		{
			OutputFormat = OutputFormat.Detailed,
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
		results.SchemaLocation.Segments.Last().Value.Should().Be("$ref");
	}

	[Test]
	public void GlobalRegistryMissesRef()
	{
		try
		{
			var options = new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed
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
			results.SchemaLocation.Segments.Last().Value.Should().Be("$ref");
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
			SchemaRegistry.Global.Fetch = _ => JsonSchema.FromText("{\"type\": \"string\", \"invalid\"}");
			var schema = JsonSchema.FromText("{\"$ref\":\"http://my.schema/test1\"}");

			using var json = JsonDocument.Parse("10");

			Assert.Throws<JsonException>(() => schema.Validate(json.RootElement));
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}
}